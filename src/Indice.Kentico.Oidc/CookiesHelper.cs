using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Indice.Kentico.Oidc
{
    public static class CookiesHelper
    {
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