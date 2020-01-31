using System;
using System.Configuration;

namespace Indice.Kentico.Oidc
{
    public static class OAuthConfiguration
    {
        public static string Authority => ConfigurationManager.AppSettings["Oidc:Authority"];
        public static string Host => ConfigurationManager.AppSettings["Oidc:Host"];
        public static string ClientId => ConfigurationManager.AppSettings["Oidc:ClientId"];
        public static string ClientSecret => ConfigurationManager.AppSettings["Oidc:ClientSecret"];
        public static string[] Scopes => ConfigurationManager.AppSettings["Oidc:Scopes"] != null ? ConfigurationManager.AppSettings["Oidc:Scopes"].Split(' ') : Array.Empty<string>();
    }
}
