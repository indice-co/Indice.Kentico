using IdentityModel;
using System.Collections.Specialized;
using System.Linq;

namespace Indice.Kentico.Oidc
{
    public class AuthorizationResponse
    {
        public string Code { get; set; }
        public string IdToken { get; set; }
        public string[] Scopes { get; set; }
        public string SessionState { get; set; }
        public string State { get; set; }

        public void PopulateFrom(NameValueCollection form)
        {
            Code = form.AllKeys.Contains(OidcConstants.AuthorizeResponse.Code) ? form[OidcConstants.AuthorizeResponse.Code] : null;
            IdToken = form.AllKeys.Contains(OidcConstants.AuthorizeResponse.IdentityToken) ? form[OidcConstants.AuthorizeResponse.IdentityToken] : null;
            Scopes = form.AllKeys.Contains(OidcConstants.AuthorizeResponse.Scope) ? form[OidcConstants.AuthorizeResponse.Scope].Split(' ') : new string[] { };
            SessionState = form.AllKeys.Contains("session_state") ? form["session_state"] : null;
            State = form.AllKeys.Contains(OidcConstants.AuthorizeResponse.State) ? form[OidcConstants.AuthorizeResponse.State] : null;
        }
    }
}
