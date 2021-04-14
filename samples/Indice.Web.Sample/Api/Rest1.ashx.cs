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
    public class Rest1 : SimpleRestHttpHandler
    {
        protected override void Configure(SimpleMVCBuilder builder) {
            builder.AddCorsAllowedOrigin("*");
        }

        public IActionResult Get() {
            return Ok(null);
        }

        
        public class Item
        {
            public int? Id { get; set; }
            public string Description { get; set; }
            public DateTime? Date { get; set; }
        }
    }
}