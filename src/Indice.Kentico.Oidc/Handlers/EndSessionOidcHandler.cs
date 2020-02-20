using CMS.Membership;
using IdentityModel;
using IdentityModel.Client;
using System.Web;

namespace Indice.Kentico.Oidc
{
    public class EndSessionOidcHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                AuthenticationHelper.SignOut();
            }
            EndSession();
        }

        public static void EndSession()
        {
            // At this point we have already sign out by using FormsAuthentication and we also have to sign out from Identity Server.
            // Create the url to Identity Server's end session endpoint.
            var endsessionEndpoint = OAuthConfiguration.Authority.TrimEnd('/') + "/connect/endsession";
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