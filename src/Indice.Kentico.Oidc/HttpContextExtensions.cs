using System.Web;

namespace Indice.Kentico.Oidc
{
    /// <summary>
    /// Http context extension methods.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Extract the token from the <see cref="CookieNames.OAuthCookie"/> name. 
        /// </summary>
        /// <param name="httpContext">The http context.</param>
        /// <param name="tokenType">The type of the token. access_token or refresh_token</param>
        /// <returns>The token value found inside of the <see cref="CookieNames.OAuthCookie"/> if any</returns>
        public static string GetToken(this HttpContext httpContext, string tokenType) => CookiesHelper.GetValue(CookieNames.OAuthCookie, tokenType);
    }
}