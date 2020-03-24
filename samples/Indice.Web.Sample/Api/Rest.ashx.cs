using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        public void GetItems(HttpContext context, int? id) {
            if (id.HasValue) {
                if (!items.ContainsKey(id.Value)) {
                    context.NotFound();
                } else {
                    context.Ok(items[id.Value]);
                }
            } else {
                context.Ok(items.Values);
            }
        }

        public void GetMyFlower(HttpContext context) {
            context.Ok(new { 
                Name =  "Camelia",
                Family = "Algea"
            });
        }

        public async Task GetPoints(HttpContext context, string center, int radius) {
            var httpClient = new HttpClient();
            var baseUrl = "";
            var response = await httpClient.GetAsync($"{baseUrl}/v1/Content/pois/All?latlong={center}&radius={radius}");
            if (!response.IsSuccessStatusCode) {
                context.BadRequest("Failed to call the remote procedure");
            }
            using (var stream = (await response.Content.ReadAsStreamAsync())) {
                await context.OkAsync(stream);
            }
        }

        public void PutItems(HttpContext context, int id, Item model) {
            if (items.ContainsKey(id)) {
                items[id] = model;
                context.Ok(model);
            } else {
                context.NotFound();
            }
        }

        public void PostItems(HttpContext context, Item model) {
            model.Id = items.Keys.Count > 0 ? (items.Keys.Max() + 1) : 0;
            model.Date = DateTime.Now;
            model.Description += $" #{model.Id:000}";
            items.Add(model.Id.Value, model);
            context.Created(model, new Uri($"https://localhost:44368/api/rest.ashx?action=sample&id={model.Id}"));
        }

        public void DeleteItems(HttpContext context, int? id) {
            if (id.HasValue && items.ContainsKey(id.Value)) {
                items.Remove(id.Value);
                context.NoContent();
            } else {
                context.BadRequest();
            }
        }

        public class Item
        {
            public int? Id { get; set; }
            public string Description { get; set; }
            public DateTime? Date { get; set; }
        }
    }
}