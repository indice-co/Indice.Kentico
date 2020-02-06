using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Indice.Kentico.Oidc
{
    public class RefreshTokenOidcHandler : IHttpHandler
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context) {
            // Read the stored refresh token:
            var refreshToken = context.GetToken(OidcConstants.TokenTypes.RefreshToken);
            if (string.IsNullOrEmpty(refreshToken)) {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.End();
            }
            var tokenEndpoint = OAuthConfiguration.Authority + "/connect/token";
            // Request a new access token.
            var tokenResponse = Task.Run(() => HttpClient.RequestRefreshTokenAsync(new RefreshTokenRequest {
                Address = tokenEndpoint,
                ClientId = OAuthConfiguration.ClientId,
                ClientSecret = OAuthConfiguration.ClientSecret,
                RefreshToken = refreshToken
            }))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
            if (tokenResponse.IsError) {
                throw new Exception(tokenResponse.Error);
            }
            // Update cookie with new values.
            CookiesHelper.SetValue(
                name: CookieNames.OAuthCookie,
                values: new Dictionary<string, string> {
                    [OidcConstants.TokenTypes.AccessToken] = tokenResponse.AccessToken,
                    [OidcConstants.TokenTypes.RefreshToken] = tokenResponse.RefreshToken,
                    [OidcConstants.TokenResponse.ExpiresIn] = tokenResponse.ExpiresIn.ToString(),
                    [OidcConstants.ResponseTypes.IdToken] = tokenResponse.IdentityToken
                },
                expires: DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn)
            );
            context.Response.ContentType = "text/json";
            context.Response.Write(JsonConvert.SerializeObject(new RenewTokenResponse {
                AccessToken = tokenResponse.AccessToken,
                ExpiresIn = tokenResponse.ExpiresIn
            },
            new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
        }
    }
}
