using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Indice.Kentico.Oidc
{
    /// <summary>
    /// Cookie helper to save and extract information response/request in an encrypted manner.
    /// </summary>
    public static class CookiesHelper
    {
        /// <summary>
        /// Saves each key value pair in the dictionary in the cookie collection after encrypting with maching key and Base64 the result.
        /// </summary>
        /// <param name="name">Name of the cookie</param>
        /// <param name="values">The data as dictionary</param>
        /// <param name="expires">Expiration</param>
        /// <param name="httpOnly"></param>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        public static void SetValue(string name, Dictionary<string, string> values, DateTime expires, bool? httpOnly = null, string domain = null, string path = "/")
        {
            var cookie = new HttpCookie(name)
            {
                HttpOnly = httpOnly ?? true,
                Expires = expires,
                Path = path
            };
            foreach (var pair in values)
            {
                cookie[pair.Key] = Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(pair.Value)));
            }
            if (!string.IsNullOrEmpty(domain))
            {
                cookie.Domain = domain;
            }
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Extract a value for a given <paramref name="key"/> from a cookie <paramref name="name"/>
        /// </summary>
        /// <param name="name">The cookie name</param>
        /// <param name="key">The key inside the dictionary</param>
        /// <returns></returns>
        /// <remarks>Data will be converted from base64 to bytes and decrypted using the machine key</remarks>
        public static string GetValue(string name, string key)
        {
            var cookie = HttpContext.Current.Request.Cookies[name];
            if (cookie != null)
            {
                var encryptedValue = cookie.Values[key];
                return Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(encryptedValue)));
            }
            return null;
        }
    }
}