using Newtonsoft.Json;
using System;
using System.Text;

namespace Indice.Kentico.Oidc
{
    public class StateProvider<T> where T : class
    {
        public string CreateState(T data)
        {
            var stateJson = JsonConvert.SerializeObject(data);
            return Base64Encode(stateJson);
        }

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