using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ecoset.GeoTemporal.Remote
{
    public class RawParser : IParser<RawDataResult>
    {
        public RawDataResult TryParse(JToken token)
        {
            var parsed = token.ToObject<RawEcosetData>();
            var result = new RawDataResult() 
            {
                Stats = parsed.Stats,
                DataCube = parsed.Data.Data,
                Columns = parsed.Data.Columns,
                Rows = parsed.Data.Rows
            };
            return result;
        }

        private class RawEcosetData
        {
            [JsonProperty("summary")]
            public Statistics Stats { get; set; }
            [JsonProperty("data")]
            public RawDataset Data { get; set; }
        }

        private class RawDataset
        {
            [JsonProperty("raw")]
            public double[,] Data { get; set; }
            [JsonProperty("ncols")]
            public double Columns { get; set; }
            [JsonProperty("nrows")]
            public double Rows { get; set; }
        }
    }
}