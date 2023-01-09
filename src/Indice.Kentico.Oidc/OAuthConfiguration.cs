using System;
using System.Configuration;
using IdentityModel;

namespace Indice.Kentico.Oidc
{
    /// <summary>
    /// Configuration members for OAuth. Each member is populated from its corresponding appSetting in the xml configuration file (web.config)
    /// </summary>
    /// <remarks>naming convention is for every property for example <see cref="ClientId"/> there will be a <strong><![CDATA[<add key = "Oidc:ClientId" value="my_portal" />]]></strong></remarks>
    public static class OAuthConfiguration
    {
        /// <summary>
        /// If true this will trigger authentication over the entire site. Otherwize the httpModule will wait for someone to go to signin.ashx handler first.
        /// </summary>
        public static bool AutoRedirect => bool.TryParse(ConfigurationManager.AppSettings["Oidc:AutoRedirect"], out var autoRedirect) && autoRedirect;
        /// <summary>
        /// A fully qualified hostname for the identity provider. (ie https://identityserver.com)
        /// </summary>
        public static string Authority =>  ConfigurationManager.AppSettings["Oidc:Authority"]?.TrimEnd('/');
        /// <summary>
        /// The fully qualified hostname for the current website. (ie https://mysite.com)
        /// </summary>
        public static string Host => ConfigurationManager.AppSettings["Oidc:Host"]?.TrimEnd('/');
        /// <summary>
        /// Client credentials 'client_id'
        /// </summary>
        public static string ClientId => ConfigurationManager.AppSettings["Oidc:ClientId"];
        /// <summary>
        /// Client credentials 'client_secret'
        /// </summary>
        public static string ClientSecret => ConfigurationManager.AppSettings["Oidc:ClientSecret"];
        /// <summary>
        /// A space separated list of scopes.  <strong>openid offline_access profile role</strong>
        /// </summary>
        public static string[] Scopes => ConfigurationManager.AppSettings["Oidc:Scopes"]?.Split(' ') ?? Array.Empty<string>();
        /// <summary>
        /// Can be either <strong>code</strong> or <strong>code id_token</strong>. The first identifies code flow and the second is the hybrid flow.
        /// </summary>
        /// <remarks>Defaults to <strong>code id_token</strong></remarks>
        public static string ResponseType => ConfigurationManager.AppSettings["Oidc:ResponseType"]?.Trim() ?? "code id_token";
        /// <summary>
        /// Username claim type. Defaults to jwt <see cref="JwtClaimTypes.Name"/> ('name'). But should change this via webconfig to 'username' in case of AWS cognito.
        /// </summary>
        public static string UserNameClaim => ConfigurationManager.AppSettings["Oidc:UserNameClaim"] ?? JwtClaimTypes.Name;
        /// <summary>
        /// Email claim type. Defaults to jwt <see cref="JwtClaimTypes.Email"/> ('email').
        /// </summary>
        public static string EmailClaim => ConfigurationManager.AppSettings["Oidc:EmailClaim"] ?? JwtClaimTypes.Email;
        /// <summary>
        /// FirstName claim type. Defaults to jwt <see cref="JwtClaimTypes.GivenName"/> ('given_name').
        /// </summary>
        public static string FirstNameClaim => ConfigurationManager.AppSettings["Oidc:FirstNameClaim"] ?? JwtClaimTypes.GivenName;
        /// <summary>
        /// LastName claim type. Defaults to jwt <see cref="JwtClaimTypes.FamilyName"/> ('family_name').
        /// </summary>
        public static string LastNameClaim => ConfigurationManager.AppSettings["Oidc:LastNameClaim"] ?? JwtClaimTypes.FamilyName;
        /// <summary>
        /// OIDC authorize endpoint. Defaults to 'connect/userinfo'
        /// </summary>
        public static string AuthorizeEndpointPath => ConfigurationManager.AppSettings["Oidc:AuthorizeEndpointPath"]?.TrimStart('/') ?? "connect/authorize";
        /// <summary>
        /// OIDC token endpoint. Defaults to 'connect/token'
        /// </summary>
        public static string TokenEndpointPath => ConfigurationManager.AppSettings["Oidc:TokenEndpointPath"]?.TrimStart('/') ?? "connect/token";
        /// <summary>
        /// OIDC userinfo endpoint. Defaults to 'connect/userinfo'
        /// </summary>
        public static string UserInfoEndpointPath => ConfigurationManager.AppSettings["Oidc:UserInfoEndpointPath"]?.TrimStart('/') ?? "connect/userinfo";
        /// <summary>
        /// OIDC end session endpoint. Defaults to 'connect/endsession'
        /// </summary>
        public static string EndSessionEndpointPath => ConfigurationManager.AppSettings["Oidc:EndSessionEndpointPath"]?.TrimStart('/') ?? "connect/endsession";
    }
}
