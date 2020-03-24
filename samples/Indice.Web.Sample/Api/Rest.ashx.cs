using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Indice.Kentico.Extensions;
using Indice.Kentico.HttpHandlers;

namespace Indice.Web.Sample.Api
{
    /// <summary>
    /// Summary description for Rest
    /// </summary>
    public class Rest : SimpleRestHttpHandler
    {
        private static readonly Dictionary<int, object> items = new Dictionary<int, object>();

        public override void Configure(Kentico.HttpHandlers.SimpleMVCBuilder builder) {
            builder.MapRoute("items2", "GET", (context) => {
                var query = context.Request.FromQuery<GetByIdQuery>();
                if (query.Id.HasValue) {
                    if (items.ContainsKey(query.Id.Value)) {
                        context.NotFound();
                    } else {
                        context.Ok(items[query.Id.Value]);
                    }
                } else {
                    context.Ok(items.Values);
                }
                return Task.CompletedTask;
            }).MapRoute("items2", "POST", (context) => {
                var model = context.Request.FromBody<Item>();
                model.Id = items.Keys.Count > 0 ? (items.Keys.Max() + 1) : 0;
                model.Date = DateTime.Now;
                model.Description += $" #{model.Id:000}";
                items.Add(model.Id.Value, model);
                context.Created(model, new Uri($"https://localhost:44368/api/rest.ashx?action=sample&id={model.Id}"));
                return Task.CompletedTask;
            }).MapRoute("items2", "DELETE", (context) => {
                var query = context.Request.FromQuery<GetByIdQuery>();
                if (query.Id.HasValue && items.ContainsKey(query.Id.Value)) {
                    items.Remove(query.Id.Value);
                    context.NoContent();
                } else {
                    context.BadRequest();
                }
                return Task.CompletedTask;
            });
        }

        public Task GetItems(HttpContext context, int? id) {
            if (id.HasValue) {
                if (items.ContainsKey(id.Value)) {
                    context.NotFound();
                } else {
                    context.Ok(items[id.Value]);
                }
            } else {
                context.Ok(items.Values);
            }
            return Task.CompletedTask;
        }

        public Task PutItems(HttpContext context, int id, Item model) {
            if (items.ContainsKey(id)) {
                items[id] = model;
                context.Ok(model);
                return Task.CompletedTask;
            } else {
                context.NotFound();
                return Task.CompletedTask;
            }
        }
        public Task PostItems(HttpContext context, Item model) {
            model.Id = items.Keys.Count > 0 ? (items.Keys.Max() + 1) : 0;
            model.Date = DateTime.Now;
            model.Description += $" #{model.Id:000}";
            items.Add(model.Id.Value, model);
            context.Created(model, new Uri($"https://localhost:44368/api/rest.ashx?action=sample&id={model.Id}"));
            return Task.CompletedTask;
        }

        public Task DeleteItems(HttpContext context, int? id) {
            if (id.HasValue && items.ContainsKey(id.Value)) {
                items.Remove(id.Value);
                context.NoContent();
            } else {
                context.BadRequest();
            }
            return Task.CompletedTask;
        }

        public class GetByIdQuery
        {
            public int? Id { get; set; }
        }
        public class Item
        {
            public int? Id { get; set; }
            public string Description { get; set; }
            public DateTime? Date { get; set; }
        }
    }
}