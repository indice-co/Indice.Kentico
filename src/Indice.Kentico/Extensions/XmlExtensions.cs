using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Indice.Kentico.Extensions
{
    public static class XmlExtensions {

        public static CultureInfo DateTimeCultureSettings = CultureInfo.InvariantCulture;

        public static T FirstOf<T>(this XElement xml, string elementName) {
            return (T)ParseValue(typeof(T), xml.Descendants(XName.Get(elementName)).FirstOrDefault()?.Value);
        }
        public static List<T> ListOf<T>(this XElement xml, string elementName) {
            return xml.Descendants(XName.Get(elementName)).Select(x => (T)ParseValue(typeof(T), x.Value)).ToList();
        }

        private static object ParseValue(Type type, string text) {
            if (typeof(string).Equals(type))
                return text;
            else if (typeof(double?).Equals(type) || typeof(double).Equals(type)) {
                if (double.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number)) {
                    return number;
                } else if (typeof(double).Equals(type)) {
                    return 0.0;
                }
            } else if (typeof(decimal?).Equals(type) || typeof(decimal).Equals(type)) {
                if (decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number)) {
                    return number;
                } else if (typeof(decimal).Equals(type)) {
                    return 0.0M;
                }
            } else if (typeof(int?).Equals(type) || typeof(int).Equals(type)) {
                if (int.TryParse(text, out var number)) {
                    return number;
                } else if (typeof(int).Equals(type)) {
                    return 0;
                }
            }
            if (typeof(int?).Equals(type) || typeof(int).Equals(type)) {
                if (int.TryParse(text, out var number)) {
                    return number;
                } else if (typeof(int).Equals(type)) {
                    return 0;
                }
            } else if (typeof(short?).Equals(type) || typeof(short).Equals(type)) {
                if (short.TryParse(text, out var number)) {
                    return number;
                } else if (typeof(short).Equals(type)) {
                    return 0;
                }
            } else if (typeof(long?).Equals(type) || typeof(long).Equals(type)) {
                if (long.TryParse(text, out var number)) {
                    return number;
                } else if (typeof(long).Equals(type)) {
                    return 0;
                }
            }
            if (typeof(bool?).Equals(type) || typeof(bool).Equals(type)) {
                if (bool.TryParse(text, out var number)) {
                    return number;
                } else if (typeof(bool).Equals(type)) {
                    return false;
                }
            } else if (typeof(DateTime?).Equals(type) || typeof(DateTime).Equals(type)) {
                if (DateTime.TryParse(text, DateTimeCultureSettings, DateTimeStyles.RoundtripKind, out var date)) {
                    return date;
                } else if (typeof(DateTime).Equals(type)) {
                    return default(DateTime);
                }
            } else if (type.IsEnum || (IsNullable(type) && type.GetGenericArguments()[0].IsEnum)) {
                if (string.IsNullOrWhiteSpace(text)) {
                    return type.IsEnum ? (object)0 : null;
                } else {
                    return Enum.Parse(type, text);
                }
            }
            return null;
        }
        private static bool IsValueType(Type type) =>
            type.IsValueType ||
            type.IsEnum ||
            type.Equals(typeof(string)) ||
            (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));

        private static bool IsNullable(Type type) => type.Equals(typeof(string)) || (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));

    }
}
