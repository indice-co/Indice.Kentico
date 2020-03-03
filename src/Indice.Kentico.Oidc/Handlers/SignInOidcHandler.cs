using CMS.Base;
using CMS.EventLog;
using CMS.Membership;
using CMS.SiteProvider;
using IdentityModel;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
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
            // Authorization endpoint will give us back 4 values.
            // i)   code:          used in order to exchange the access token
            // ii)  id_token:      contains user's authentication information in an encoded format
            // iii) scope:         the access privileges requested for access token
            // iv)  session_state: allows you to restore the previous state of your application
            authorizationResponse.PopulateFrom(context.Request.Form);
            // Check if authorization code is present in the response.
            if (string.IsNullOrEmpty(authorizationResponse.Code)) {
                throw new Exception("Authorization code is not present in the response.");
            }
            var tokenEndpoint = OAuthConfiguration.Authority + "/connect/token";
            var userInfoEndpoint = OAuthConfiguration.Authority + "/connect/userinfo";
            // Finally exchange the authorization code with the access token.
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
            // Get user claims by calling the user info endpoint using the access token.
            var userInfoResponse = Task.Run(() => HttpClient.GetUserInfoAsync(new UserInfoRequest {
                Address = userInfoEndpoint,
                Token = tokenResponse.AccessToken
            }))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
            if (userInfoResponse.IsError) {
                throw new Exception("There was an error retrieving user information from authority.", userInfoResponse.Exception);
            }
            // It is important to get the email claim and check if the user exists locally.
            var userClaims = userInfoResponse.Claims;
            var userName = userClaims.GetValueOrDefault(JwtClaimTypes.Name);
            var email = userClaims.GetValueOrDefault(JwtClaimTypes.Email);
            if (string.IsNullOrEmpty(userName)) {
                throw new Exception("Email cannot be found in user claims.");
            }
            // Check if the user exists in Kentico.
            var userInfo = UserInfoProvider.GetUserInfo(userName);
            // Get admin claim so we can decide if we need to assign a specific role to the user. 
            var isAdmin = userClaims.GetValueOrDefault<bool>(CustomClaimTypes.Admin);
            // In this case we need to create the user.
            if (userInfo == null) {
                var firstName = userClaims.GetValueOrDefault(JwtClaimTypes.GivenName);
                var lastName = userClaims.GetValueOrDefault(JwtClaimTypes.FamilyName);
                // Creates a new user object.
                userInfo = new UserInfo {
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
                    UserInfoProvider.SetUserInfo(userInfo);
                }
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
                returnUrl = state;
            }
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
