using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace Indice.Kentico.Oidc
{
    public static class ClaimExtensions
    {
        public static T GetValueOrDefault<T>(this IEnumerable<Claim> claims, string claimType) where T : struct
        {
            var result = default(T);
            var valueString = claims.FirstOrDefault(x => x.Type == claimType)?.Value;
            object value = default(T);
            if (valueString == null)
            {
                result = (T)value;
                return result;
            }
            var type = typeof(T);
            if (type.GetTypeInfo().IsEnum)
            {
                value = Enum.Parse(type, valueString, true);
            }
            else if (type == typeof(bool))
            {
                value = bool.Parse(valueString);
            }
            else if (type == typeof(int))
            {
                value = int.Parse(valueString);
            }
            else if (type == typeof(Guid))
            {
                value = Guid.Parse(valueString);
            }
            else if (type == typeof(double))
            {
                value = double.Parse(valueString, CultureInfo.InvariantCulture);
            }
            else if (type == typeof(DateTime))
            {
                value = DateTime.Parse(valueString, CultureInfo.InvariantCulture);
            }
            else if (type == typeof(TimeSpan))
            {
                value = TimeSpan.Parse(valueString, CultureInfo.InvariantCulture);
            }
            result = (T)value;
            return result;
        }

        public static string GetValueOrDefault(this IEnumerable<Claim> claims, string claimType) => claims.FirstOrDefault(x => x.Type == claimType)?.Value;
    }
}