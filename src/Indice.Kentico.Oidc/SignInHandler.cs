using System.Web;

namespace Indice.Kentico.Oidc
{
    public class SignInHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context) {
            var returnUrl = context.Request.QueryString["returnUrl"];
            OidcAuthenticationModule.RedirectToAuthority(returnUrl);
        }
    }
}
