using IdentityModel;
using System.Collections.Specialized;
using System.Linq;

namespace Indice.Kentico.Oidc
{
    /// <summary>
    /// The response of the authorization process after hiting the signin callback handler.
    /// </summary>
    public class AuthorizationResponse
    {
        /// <summary>
        /// code. used in order to exchange the access token
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// id_token. contains user's information profile claims.
        /// </summary>
        public string IdToken { get; set; }
        /// <summary>
        /// token. contains user's access token.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// scope member of the authorization response.
        /// </summary>
        public string[] Scopes { get; set; }
        /// <summary>
        /// state. provide by us; allows you to restore the previous state of your application
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Populate through a <see cref="NameValueCollection"/> (QueryString or Form)
        /// </summary>
        /// <param name="collection"></param>
        public void PopulateFrom(NameValueCollection collection)
        {
            Code = collection.AllKeys.Contains(OidcConstants.AuthorizeResponse.Code) ? collection[OidcConstants.AuthorizeResponse.Code] : null;
            IdToken = collection.AllKeys.Contains(OidcConstants.AuthorizeResponse.IdentityToken) ? collection[OidcConstants.AuthorizeResponse.IdentityToken] : null;
            Scopes = collection.AllKeys.Contains(OidcConstants.AuthorizeResponse.Scope) ? collection[OidcConstants.AuthorizeResponse.Scope].Split(' ') : new string[] { };
            State = collection.AllKeys.Contains(OidcConstants.AuthorizeResponse.State) ? collection[OidcConstants.AuthorizeResponse.State] : 
                    collection.AllKeys.Contains("session_state") ? collection["session_state"] : null;
        }
    }
}
