using CMS.Base;
using CMS.EventLog;
using CMS.Membership;
using CMS.SiteProvider;
using IdentityModel;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace Indice.Kentico.Oidc
{
    public class SignInOidcHandler : IHttpHandler
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public SignInOidcHandler() { }

        public bool IsReusable => false;
        public static event EventHandler<UserCreatedEventArgs> UserCreated;

        public void ProcessRequest(HttpContext context) {
            var authorizationResponse = new AuthorizationResponse();
            // If response_type is "code id_token", the authorization endpoint will give us back 
            //4 values.
            // i)   code:          used in order to exchange the access token
            // ii)  id_token:      contains user's authentication information in an encoded format
            // iii) scope:         the access privileges requested for access token
            // iv)  session_state: allows you to restore the previous state of your application
            //
            // If response_type is "code", the authorization endpoint will give us back 2 values:
            // i)   code:          used in order to exchange the access token
            // ii)  state:         provide by us; allows you to restore the previous state of your application

            // Begin by determining whether authorization (code) or hybrid flow (code id_token)

            if (OAuthConfiguration.ResponseType == "CodeIdToken") {
                authorizationResponse.PopulateFrom(context.Request.Form);
            } else {
                authorizationResponse.Code = context.Request.QueryString["code"];
                authorizationResponse.State = context.Request.QueryString["state"];
            }

            // Check if authorization code is present in the response.
            if (string.IsNullOrEmpty(authorizationResponse.Code)) {
                throw new Exception("Authorization code is not present in the response.");
            }
            var tokenEndpoint = OAuthConfiguration.Authority + "/" + OAuthConfiguration.TokenEndpointPath;
            var userInfoEndpoint = OAuthConfiguration.Authority + "/" +OAuthConfiguration.UserInfoEndpointPath;

            // Use the authorization code to retrieve access and id tokens.
            var tokenResponse = Task.Run(() => HttpClient.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest {
                Address = tokenEndpoint,
                ClientId = OAuthConfiguration.ClientId,
                ClientSecret = OAuthConfiguration.ClientSecret,
                Code = authorizationResponse.Code,
                RedirectUri = $"{OAuthConfiguration.Host}/SignInOidc.ashx"
            }))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
 
            if (tokenResponse.IsError) {
                throw new Exception("There was an error retrieving the access token.", tokenResponse.Exception);
            }

            // If using an authorization code flow, we get the id_token from the token endpoint
            // so we populate it now into the authorizationResponse object
            if (OAuthConfiguration.ResponseType == "Code") {
                authorizationResponse.IdToken = tokenResponse.Json["id_token"].ToString();
            }
            // Get user claims by calling the user info endpoint using the access token.
            var userInfoResponse = Task.Run(() => HttpClient.GetUserInfoAsync(new UserInfoRequest {
                Address = userInfoEndpoint,
                Token = tokenResponse.AccessToken
            }))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

            //LOGGING//
            StreamWriter sw5 = new StreamWriter("c:\\docs\\logfile1.txt", append: true);
            sw5.WriteLine("The token response is: " + tokenResponse.Json);
            sw5.WriteLine("The endpoint is: " + userInfoEndpoint);
            sw5.WriteLine("The error is: " + userInfoResponse.Error);
            sw5.WriteLine("The raw is: " + userInfoResponse.Raw);
            sw5.Close();
            //END LOGGING//

            if (userInfoResponse.IsError) {
                throw new Exception("There was an error retrieving user information from authority.", userInfoResponse.Exception);
            }
            // It is important to get the email claim and check if the user exists locally.
            var userClaims = userInfoResponse.Claims;

            //Commented out from original code
            //var userName = userClaims.GetValueOrDefault(OAuthConfiguration.UserNameClaim ?? JwtClaimTypes.Name);
            var userName = userInfoResponse.Json[OAuthConfiguration.UserNameClaim].ToString();
            var email = userClaims.GetValueOrDefault(JwtClaimTypes.Email);
            if (string.IsNullOrEmpty(userName)) {
                throw new Exception("Username cannot be found in user claims.");
            }
            // Check if the user exists in Kentico.
            UserInfo userInfo = UserInfoProvider.GetUserInfo(userName);

            // Get admin claim so we can decide if we need to assign a specific role to the user. 
            var isAdmin = userClaims.GetValueOrDefault<bool>(CustomClaimTypes.Admin);

            // In this case we need to create the user.
            if (userInfo == null) {
                var firstName = userClaims.GetValueOrDefault(JwtClaimTypes.GivenName);
                var lastName = userClaims.GetValueOrDefault(JwtClaimTypes.FamilyName);

                // Creates a new user object.
                userInfo = new UserInfo{
                    // Sets the user properties.
                    Email = email,
                    Enabled = true,
                    FirstName = firstName,
                    FullName = $"{firstName} {lastName}",
                    IsExternal = true,
                    LastName = lastName,
                    SiteIndependentPrivilegeLevel = isAdmin ? UserPrivilegeLevelEnum.GlobalAdmin : UserPrivilegeLevelEnum.None,
                    UserCreated = DateTime.UtcNow,
                    UserName = userName,
                    UserIsDomain = true
                };

                // Created user must first be created and saved so we can update other properties in the next steps.
                UserInfoProvider.SetUserInfo(userInfo);
                UserSiteInfoProvider.AddUserToSite(userInfo.UserID, SiteContext.CurrentSite.SiteID);
                var handler = UserCreated;
                handler?.Invoke(this, new UserCreatedEventArgs {
                    User = userInfo,
                    Claims = userClaims
                });
            } else {
                // Update existing user's privilege level to reflect a possible change made on IdentityServer.
                if (isAdmin) {
                    userInfo.SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.GlobalAdmin;
                }
                userInfo.UserIsDomain = true;
                var userCurrentSite = UserSiteInfoProvider.GetUserSiteInfo(userInfo.UserID, SiteContext.CurrentSiteID);
                if (userCurrentSite == null) {
                    UserSiteInfoProvider.AddUserToSite(userInfo.UserID, SiteContext.CurrentSiteID);
                }
                UserInfoProvider.SetUserInfo(userInfo);
            }
            // Log the user in.
            AuthenticateUser(userInfo.UserName, true);
            CookiesHelper.SetValue(
                name: CookieNames.OAuthCookie,
                values: new Dictionary<string, string> {
                    { OidcConstants.TokenTypes.AccessToken, tokenResponse.AccessToken },
                    { OidcConstants.TokenTypes.RefreshToken, tokenResponse.RefreshToken },
                    { OidcConstants.TokenResponse.ExpiresIn, tokenResponse.ExpiresIn.ToString() },
                    { OidcConstants.ResponseTypes.IdToken, tokenResponse.IdentityToken }
                },
                expires: DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn)
            );
            // Try to retrieve state in order to navigate the user back to where he initially requested.
            var returnUrl = "/";
            if (!string.IsNullOrEmpty(authorizationResponse.State)) {
                var stateProvider = new StateProvider<string>();
                var state = stateProvider.RetrieveState(authorizationResponse.State);
                if (state != "") {
                    returnUrl = state;
                }
                else {
                    returnUrl = OAuthConfiguration.Host;
                }
            }
            //LOGGING//
            StreamWriter sw6 = new StreamWriter("c:\\docs\\logfile1.txt", append: true);
            sw6.WriteLine("The URL is: " + returnUrl);
            sw6.Close();
            //END LOGGING//

            // Redirect to the requested page.
            context.Response.Redirect(returnUrl);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private void AuthenticateUser(string userName, bool createPersistentCookie, bool loadCultures = true) {
            var userInfo = UserInfoProvider.GetUserInfo(userName);
            if (SecurityEvents.Authenticate.IsBound) {
                SecurityEvents.Authenticate.StartEvent(ref userInfo, userName, string.Empty, string.Empty, null);
            }
            if (userInfo == null) {
                return;
            }
            // Represents the active login session.
            var sessionId = Guid.NewGuid();
            var ticket = new FormsAuthenticationTicket(1, userInfo.UserName, DateTime.Now, DateTime.Now.AddMinutes(30), createPersistentCookie, sessionId.ToString(), FormsAuthentication.FormsCookiePath);
            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket));
            MembershipContext.AuthenticatedUser = new CurrentUserInfo(userInfo, true);
            if (loadCultures) {
                UserInfoProvider.SetPreferredCultures(userInfo);
            }
            var request = HttpContext.Current.Request;
            EventLogProvider.LogEvent(
                eventType: "I",
                source: "Authentication",
                eventCode: "AUTHENTICATED",
                eventUrl: request.RawUrl,
                userId: userInfo.UserID,
                userName: userInfo.UserName,
                ipAddress: request.UserHostAddress,
                siteId: SiteContext.CurrentSiteID,
                eventTime: DateTime.UtcNow
            );
            AuthenticationHelper.FinalizeAuthenticationProcess(userInfo, SiteContext.CurrentSiteID);
        }
    }
}
