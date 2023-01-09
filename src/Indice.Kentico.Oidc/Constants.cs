namespace Indice.Kentico.Oidc
{
    /// <summary>
    /// Custom indice specific claims
    /// </summary>
    public static class CustomClaimTypes
    {
        /// <summary>
        /// The admin claim. True if global admin otherwize false.
        /// </summary>
        public const string Admin = "admin";
    }

    /// <summary>
    /// Cookie names
    /// </summary>
    public static class CookieNames
    {
        /// <summary>
        /// Cookie name to use for OAuth.
        /// </summary>
        public const string OAuthCookie = nameof(OAuthCookie);
    }
}
