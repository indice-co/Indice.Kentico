using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Indice.Kentico.Extensions
{
    public static class XmlExtensions {

        public static CultureInfo DefaultCultureSettings = CultureInfo.InvariantCulture;

        public static T FirstOf<T>(this XElement xml, string elementName, string format = null) => (T)FirstOf(xml, elementName, typeof(T), format);
        public static List<T> ListOf<T>(this XElement xml, string elementName, string format = null) => ListOf(xml, elementName, typeof(T), format).Select(x => (T)x).ToList();

        public static string FirstOf(this XElement xml, string elementName) => FirstOf<string>(xml, elementName);
        public static List<string> ListOf(this XElement xml, string elementName) => ListOf<string>(xml, elementName);

        public static object FirstOf(this XElement xml, string elementName, Type type, string format = null) {
            return ParseValue(type, xml.Descendants(XName.Get(elementName)).FirstOrDefault()?.Value, format);
        }
        public static IEnumerable<object> ListOf(this XElement xml, string elementName, Type type, string format = null) {
            return xml.Descendants(XName.Get(elementName)).Select(x => ParseValue(type, x.Value, format));
        }

        private static object ParseValue(Type type, string text, string format = null) {
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
                } else if (!string.IsNullOrEmpty(text)) {
                    return text == "1";
                } else if (typeof(bool).Equals(type)) {
                    return false;
                }
            } else if (typeof(DateTime?).Equals(type) || typeof(DateTime).Equals(type)) {
                var date = default(DateTime);
                var success = string.IsNullOrEmpty(format) ? DateTime.TryParse(text, DefaultCultureSettings, DateTimeStyles.RoundtripKind, out date)
                                                           : DateTime.TryParseExact(text, format, DefaultCultureSettings, DateTimeStyles.RoundtripKind, out date);
                if (success) {
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
