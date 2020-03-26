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

        public IActionResult GetItems(int? id) {
            if (id.HasValue) {
                if (!items.ContainsKey(id.Value)) {
                    return NotFound();
                } else {
                    return Ok(items[id.Value]);
                }
            } else {
                return Ok(items.Values);
            }
        }

        public IActionResult GetMyFlower() {
            return Ok(new { 
                Name =  "Camelia",
                Family = "Algea"
            });
        }

        public async Task<IActionResult> GetPoints(string center, int radius) {
            var httpClient = new HttpClient();
            var baseUrl = "";
            var response = await httpClient.GetAsync($"{baseUrl}/v1/Content/pois/All?latlong={center}&radius={radius}");
            if (!response.IsSuccessStatusCode) {
                return BadRequest("Failed to call the remote procedure");
            }
            using (var stream = (await response.Content.ReadAsStreamAsync())) {
                return Ok(stream);
            }
        }

        public IActionResult PutItems(int id, Item model) {
            if (items.ContainsKey(id)) {
                items[id] = model;
                return Ok(model);
            } else {
                return NotFound();
            }
        }

        public IActionResult PostItems(Item model) {
            model.Id = items.Keys.Count > 0 ? (items.Keys.Max() + 1) : 0;
            model.Date = DateTime.Now;
            model.Description += $" #{model.Id:000}";
            items.Add(model.Id.Value, model);
            return Created(model, new Uri($"https://localhost:44368/api/rest.ashx?action=sample&id={model.Id}"));
        }

        public IActionResult DeleteItems(int? id) {
            if (id.HasValue && items.ContainsKey(id.Value)) {
                items.Remove(id.Value);
                return NoContent();
            } else {
                return BadRequest();
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