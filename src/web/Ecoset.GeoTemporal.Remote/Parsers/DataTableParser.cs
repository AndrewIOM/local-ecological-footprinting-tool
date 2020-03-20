using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ecoset.GeoTemporal.Remote
{
    public class DataTableParser : IParser<DataTableListResult>
    {
        public DataTableListResult TryParse(JToken token)
        {
            var parsed = token.ToObject<List<Dictionary<string,string>>>();
            var model = new DataTableListResult();
            model.Rows = parsed;
            return model;
        }
    }

    public class DataTableStatsParser : IParser<DataTableStatsResult>
    {
        public DataTableStatsResult TryParse(JToken token)
        {
            var parsed = token.ToObject<RawEcosetSummaryData>();
            return new DataTableStatsResult() 
            {
                Stats = parsed.Stats
            };
        }

        private class RawEcosetSummaryData
        {
            [JsonProperty("summary")]
            public Statistics Stats { get; set; }
        }
    }

    public class CsvParser : IParser<CsvFileResult>
    {
        public CsvFileResult TryParse(JToken raw)
        {
            var parser = new DataTableParser();
            var ob = parser.TryParse(raw);
            
            if (ob.Rows.Count == 0) return new CsvFileResult { CsvData = "" };

            var colNames = ob.Rows.First().Select(m => m.Key).ToList();
            var rowData = ob.Rows.Select(m => string.Join(",", m.Select(n => n.Value))).ToList();
            var csvData = string.Join(",", colNames) + "\n" + string.Join("\n", rowData);
            return new CsvFileResult { CsvData = csvData };
        }
    }


}