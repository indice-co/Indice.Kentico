using System.Web;

namespace Indice.Kentico.Oidc
{
    /// <summary>
    /// The sign in handler. <strong>/SignIn.ashx</strong>
    /// </summary>
    public class SignInHandler : IHttpHandler
    {
        /// <inheritdoc/>
        public bool IsReusable => false;

        /// <inheritdoc/>
        public void ProcessRequest(HttpContext context) {
            var returnUrl = context.Request.QueryString["returnUrl"];
            OidcAuthenticationModule.RedirectToAuthority(returnUrl);
        }
    }
}
