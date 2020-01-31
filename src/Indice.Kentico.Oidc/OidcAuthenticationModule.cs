using CMS.Helpers;
using IdentityModel;
using IdentityModel.Client;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;

namespace Indice.Kentico.Oidc
{
    public class OidcAuthenticationModule : IHttpModule
    {
        public OidcAuthenticationModule() { }

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += AuthenticateRequest;
            context.EndRequest += EndRequest;
        }

        public void Dispose() { }

        private void AuthenticateRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.User != null)
            {
                return;
            }
            var hasAuthCookie = HttpContext.Current.Request.Cookies.AllKeys.Contains(FormsAuthentication.FormsCookieName);
            if (!hasAuthCookie)
            {
                return;
            }
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            var principal = new ClaimsPrincipal(new FormsIdentity(authTicket));
            SetPrincipal(principal);
        }

        private void EndRequest(object sender, EventArgs e)
        {
            var response = HttpContext.Current.Response;
            // EndRequest is the last event in the pipeline. If status code is 401 then we are going to a protected area. 
            if (response.StatusCode == 401)
            {
                RedirectToAuthority();
            }
        }

        private void RedirectToAuthority()
        {
            var authorizeEndpoint = OAuthConfiguration.Authority + "/connect/authorize";
            var stateProvider = new StateProvider<string>();
            var currentPath = HttpContext.Current.Request.RawUrl;
            var requestUrl = new RequestUrl(authorizeEndpoint);
            // Create the url to Identity Server's authorize endpoint.
            var authorizeUrl = requestUrl.CreateAuthorizeUrl(
                clientId: OAuthConfiguration.ClientId,
                responseType: OidcConstants.ResponseTypes.CodeIdToken, // Requests an authorization code and identity token.
                responseMode: OidcConstants.ResponseModes.FormPost, // Sends the token response as a form post instead of a fragment encoded redirect.
                redirectUri: $"{OAuthConfiguration.Host}/SignInOidc.ashx",
                nonce: Guid.NewGuid().ToString(), // Identity Server will echo back the nonce value in the identity token (this is for replay protection).
                scope: OAuthConfiguration.Scopes.Join(" "),
                state: stateProvider.CreateState(currentPath)
            );
            if (!HttpContext.Current.Response.IsRequestBeingRedirected)
            {
                // Redirect the user to the authority.
                HttpContext.Current.Response.Redirect(authorizeUrl);
            }
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }
    }
}
