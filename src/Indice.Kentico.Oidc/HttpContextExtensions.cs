using System.Web;

namespace Indice.Kentico.Oidc
{
    public static class HttpContextExtensions
    {
        public static string GetToken(this HttpContext httpContext, string tokenType) => CookiesHelper.GetValue(CookieNames.OAuthCookie, tokenType);
    }
}