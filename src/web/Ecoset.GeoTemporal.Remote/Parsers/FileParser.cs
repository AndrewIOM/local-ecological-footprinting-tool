using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ecoset.GeoTemporal.Remote
{
    public class Base64Parser : IParser<Base64FileResult>
    {
        public Base64FileResult TryParse(string raw)
        {
            var parsed = JsonConvert.DeserializeObject<Base64EcosetResult>(raw);
            var model = new Base64FileResult();
            model.Base64Data = parsed.Base64;
            //model.FileFormat = parsed.FileFormat;
            return model;
        }

        private class Base64EcosetResult 
        {
            [JsonProperty("base64")]
            public string Base64 { get; set; }
            //public string FileFormat { get; set; }
        }
    }
}