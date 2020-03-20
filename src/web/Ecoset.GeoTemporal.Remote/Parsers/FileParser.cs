using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ecoset.GeoTemporal.Remote
{
    public class Base64Parser : IParser<Base64FileResult>
    {
        public Base64FileResult TryParse(JToken token)
        {
            var parsed = token.ToObject<Base64EcosetResult>();
            return new Base64FileResult() {
                Base64Data = parsed.Base64
            };
        }

        private class Base64EcosetResult 
        {
            [JsonProperty("base64")]
            public string Base64 { get; set; }
        }
    }
}