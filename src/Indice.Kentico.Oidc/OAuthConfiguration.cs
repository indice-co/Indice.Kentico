using System;
using Microsoft.Azure;

namespace Indice.Kentico.Oidc
{
    public static class OAuthConfiguration
    {
        public static bool AutoRedirect => bool.TryParse(CloudConfigurationManager.GetSetting("Oidc:AutoRedirect"), out var autoRedirect) ? autoRedirect : false;
        public static string Authority => CloudConfigurationManager.GetSetting("Oidc:Authority");
        public static string Host => CloudConfigurationManager.GetSetting("Oidc:Host");
        public static string ClientId => CloudConfigurationManager.GetSetting("Oidc:ClientId");
        public static string ClientSecret => CloudConfigurationManager.GetSetting("Oidc:ClientSecret");
        public static string[] Scopes => CloudConfigurationManager.GetSetting("Oidc:Scopes")?.Split(' ') ?? Array.Empty<string>();
    }
}
