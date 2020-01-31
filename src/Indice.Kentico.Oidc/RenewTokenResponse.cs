namespace Indice.Kentico.Oidc
{
    public class RenewTokenResponse
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}