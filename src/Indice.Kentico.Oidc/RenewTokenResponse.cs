namespace Indice.Kentico.Oidc
{
    /// <summary>
    /// Renew token response payload
    /// </summary>
    public class RenewTokenResponse
    {
        /// <summary>
        /// The access_token
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// Expiration in seconds.
        /// </summary>
        public int ExpiresIn { get; set; }
    }
}