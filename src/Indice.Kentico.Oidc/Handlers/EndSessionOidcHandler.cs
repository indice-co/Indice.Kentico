using CMS.Membership;
using IdentityModel;
using IdentityModel.Client;
using System.Web;

namespace Indice.Kentico.Oidc
{

    /// <summary>
    /// The signout handler. <strong>/SignOut.ashx</strong>
    /// </summary>
    public class EndSessionOidcHandler : IHttpHandler
    {
        /// <inheritdoc/>
        public bool IsReusable => false;

        /// <inheritdoc/>
        public void ProcessRequest(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                AuthenticationHelper.SignOut();
            }
            EndSession();
        }

        private static void EndSession() {
            // At this point we have already sign out by using FormsAuthentication and we also have to sign out from Identity Server.
            // Create the url to Identity Server's end session endpoint.
            var endsessionEndpoint = OAuthConfiguration.Authority + "/" + OAuthConfiguration.EndSessionEndpointPath;
            var requestUrl = new RequestUrl(endsessionEndpoint);
            var endSessionUrl = requestUrl.CreateEndSessionUrl(
                idTokenHint: HttpContext.Current.GetToken(OidcConstants.ResponseTypes.IdToken),
                postLogoutRedirectUri: OAuthConfiguration.Host
            );
            if (!HttpContext.Current.Response.IsRequestBeingRedirected)
            {
                HttpContext.Current.Response.Redirect(endSessionUrl);
            }
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }
}