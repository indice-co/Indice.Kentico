using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Indice.Kentico.Extensions
{
    public static class ApiExtesions
    {
        public static readonly JsonSerializerSettings JsonSettings;
        static ApiExtesions() {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver() {
                NamingStrategy = new CamelCaseNamingStrategy {
                    ProcessDictionaryKeys = false,
                    OverrideSpecifiedNames = false
                }
            };
            //settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter { CamelCaseText = false });
            //settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            JsonSettings = settings;
        }

        /// <summary>
        ///     A NameValueCollection extension method that converts the @this to a dictionary.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an IDictionary&lt;string,object&gt;</returns>
        public static IDictionary<string, object> ToDictionary(this NameValueCollection @this) {
            var dict = new Dictionary<string, object>();

            if (@this != null) {
                foreach (string key in @this.AllKeys) {
                    dict.Add(key, @this[key]);
                }
            }

            return dict;
        }

        public static T FromQuery<T>(this HttpRequest request) where T : class {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(request.QueryString.ToDictionary()));
        }

        public static T FromForm<T>(this HttpRequest request) where T : class {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(request.Form.ToDictionary()));
        }

        public static T FromBody<T>(this HttpRequest request) where T : class {
            using (var reader = new StreamReader(request.InputStream))
            using (JsonTextReader jsonReader = new JsonTextReader(reader)) {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }

        public static void Ok(this HttpContext context, object body) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Write(JsonConvert.SerializeObject(body, JsonSettings));
        }

        public static void Created(this HttpContext context, object body, Uri location) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 201;
            context.Response.RedirectLocation = location.ToString();
            context.Response.Write(JsonConvert.SerializeObject(body, JsonSettings));
        }

        public static void BadRequest(this HttpContext context, string message = "", string code = null) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 400;
            context.Response.Write(JsonConvert.SerializeObject(new {
                status = 400,
                title = "Bad Request",
                detail = message,
                code = code
            }, JsonSettings));
        }
        public static void NotFound(this HttpContext context, string message = "", string code = null) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 404;
            context.Response.Write(JsonConvert.SerializeObject(new {
                status = 404,
                title = "Not Found",
                detail = message,
                code = code
            }, JsonSettings));
        }
        public static void Forbidden(this HttpContext context, string message = "", string code = null) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 403;
            context.Response.Write(JsonConvert.SerializeObject(new {
                status = 403,
                title = "Forbidden",
                detail = message,
                code = code
            }, JsonSettings));
        }
        public static void Unauthorized(this HttpContext context, string message = "", string code = null) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 401;
            context.Response.Write(JsonConvert.SerializeObject(new {
                status = 401,
                title = "Unauthorized",
                detail = message,
                code = code
            }, JsonSettings));
        }

        public static void ServerError(this HttpContext context, string message = "", string code = null) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            context.Response.Write(JsonConvert.SerializeObject(new {
                status = 500,
                title = "Server Error",
                detail = message,
                code = code
            }, JsonSettings));
        }

        public static void NoContent(this HttpContext context) {
            context.Response.StatusCode = 204;
        }
    }
}
