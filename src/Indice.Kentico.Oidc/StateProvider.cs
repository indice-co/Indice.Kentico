using Newtonsoft.Json;
using System;
using System.Text;

namespace Indice.Kentico.Oidc
{
    /// <summary>
    /// State helper in order to serialize and send back and forth while authenticating with the identity provider.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StateProvider<T> where T : class
    {
        /// <summary>
        /// serialize state
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string CreateState(T data)
        {
            var stateJson = JsonConvert.SerializeObject(data);
            return Base64Encode(stateJson);
        }

        /// <summary>
        /// deserialize state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public T RetrieveState(string state)
        {
            var decodedState = Base64Decode(state);
            return JsonConvert.DeserializeObject<T>(decodedState);
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}