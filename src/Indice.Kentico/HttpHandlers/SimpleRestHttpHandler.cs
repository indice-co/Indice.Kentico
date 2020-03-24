using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Indice.Kentico.Extensions;

namespace Indice.Kentico.HttpHandlers
{
    /// <summary>
    /// Summary description for Rest
    /// </summary>
    public abstract class SimpleRestHttpHandler : HttpTaskAsyncHandler
    {
        private static readonly string [] VERBS = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD" };
        private static readonly Regex PASCAL_CASE_EXPRESSION = new Regex("(?<=[A-Za-z])(?=[A-Z][a-z])|(?<=[a-z0-9])(?=[0-9]?[A-Z])");
        private readonly SimpleMVCBuilder _builder;
        public SimpleRestHttpHandler() {
            _builder = new SimpleMVCBuilder();
            ConfigureInternal();
        }

        public override Task ProcessRequestAsync(HttpContext context) {
            return _builder.ProcessRequestAsync(context);
        }

        private void ConfigureInternal() {
            DiscoverRoutesByConvention();
            Configure(_builder);
        }

        public virtual void Configure(SimpleMVCBuilder builder) { 
        
        }

        public override bool IsReusable => true;

        public void DiscoverRoutesByConvention() {
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in methods) {
                if (VERBS.Any(v => method.Name.StartsWith(v, StringComparison.OrdinalIgnoreCase))) {
                    var parts = PASCAL_CASE_EXPRESSION.Split(method.Name);
                    var verb = parts[0].ToUpper();
                    var action = string.Join("-", parts.Skip(1)).ToLower();
                    _builder.MapRoute(action, verb, (context) => HandleActionByConventionAsync(context, method));
                }
            }
        }

        private Task HandleActionByConventionAsync(HttpContext context, MethodInfo method) {
            var isAwaitable = typeof(Task).IsAssignableFrom(method.ReturnType);
            var parameters = method.GetParameters();
            var arguments = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++) {
                var type = parameters[i].ParameterType;
                if (typeof(HttpContext).Equals(type)) {
                    arguments[i] = context;
                } else if (IsValueType(type)) {
                    var text = context.Request.QueryString[parameters[i].Name];
                    arguments[i] = ParseValue(type, text);
                } else {
                    if (context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase)) {
                        arguments[i] = context.Request.FromQuery(type);
                    } else if ("application/x-www-form-urlencoded".Equals(context.Request.ContentType)) {
                        arguments[i] = context.Request.FromForm(type);
                    } else {
                        arguments[i] = context.Request.FromBody(type);
                    }
                }
            }
            if (isAwaitable) {
                return (Task)method.Invoke(this, arguments.ToArray());
            } else {
                method.Invoke(this, arguments.ToArray());
                return Task.CompletedTask;
            }
        }

        private object ParseValue(Type type, string text) {
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
            } else if (typeof(DateTime?).Equals(type) || typeof(DateTime).Equals(type)) {
                if (DateTime.TryParse(text, out var date)) {
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

        private bool IsValueType(Type type) => 
            type.IsValueType || 
            type.IsEnum || 
            type.Equals(typeof(string)) || 
            (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));

        private bool IsNullable(Type type) => type.Equals(typeof(string)) || (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
    }

    public class SimpleMVCBuilder
    {
        protected Dictionary<string, Func<HttpContext, Task>> Routes { get; private set; } = new Dictionary<string, Func<HttpContext, Task>>(StringComparer.OrdinalIgnoreCase);

        public SimpleMVCBuilder MapRoute(string action, string verb, Func<HttpContext, Task> handler) {
            Routes.Add($"{verb.ToUpper()} {action.ToLower()}", handler);
            return this;
        }

        internal async Task ProcessRequestAsync(HttpContext context) {
            var query = context.Request.QueryString;
            var action = query["action"]?.ToLower();
            context.Response.ContentType = "application/json";
            string requestUrl = $"{context.Request.HttpMethod.ToUpper()} {action.ToLower()}";
            try {
                Func<HttpContext, Task> handler;
                if (Routes.ContainsKey(requestUrl)) {
                    handler = Routes[requestUrl];
                    await handler(context);
                    return;
                } else if (context.Request.HttpMethod == "HEAD" && Routes.ContainsKey($"GET {action.ToLower()}")) {
                    handler = Routes[$"GET {context.Request.Path.ToLower()}"];
                    await handler(context);
                    return;
                } else {
                    context.NotFound();
                    return;
                }
            } catch (Exception ex) {
                context.ServerError(ex.ToString());
            }
        }
    }
}
