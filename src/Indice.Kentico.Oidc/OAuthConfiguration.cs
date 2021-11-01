using System;
using System.Configuration;
using IdentityModel;

namespace Indice.Kentico.Oidc
{
    public static class OAuthConfiguration
    {
        public static bool AutoRedirect => bool.TryParse(ConfigurationManager.AppSettings["Oidc:AutoRedirect"], out var autoRedirect) ? autoRedirect : false;
        public static string Authority =>  ConfigurationManager.AppSettings["Oidc:Authority"];
        public static string Host => ConfigurationManager.AppSettings["Oidc:Host"];
        public static string ClientId => ConfigurationManager.AppSettings["Oidc:ClientId"];
        public static string ClientSecret => ConfigurationManager.AppSettings["Oidc:ClientSecret"];
        public static string[] Scopes => ConfigurationManager.AppSettings["Oidc:Scopes"]?.Split(' ') ?? Array.Empty<string>();
        public static string ResponseType => ConfigurationManager.AppSettings["Oidc:ResponseType"];
        public static string UserNameClaim => ConfigurationManager.AppSettings["Oidc:UserNameClaim"] ?? "username"; 
        public static string AuthorizeEndpointPath => ConfigurationManager.AppSettings["Oidc:AuthorizeEndpointPath"]?.TrimStart('/') ?? "connect/authorize";
        public static string TokenEndpointPath => ConfigurationManager.AppSettings["Oidc:TokenEndpointPath"]?.TrimStart('/') ?? "connect/authorize";
        public static string UserInfoEndpointPath => ConfigurationManager.AppSettings["Oidc:UserInfoEndpointPath"]?.TrimStart('/') ?? "connect/authorize";
    }
}
