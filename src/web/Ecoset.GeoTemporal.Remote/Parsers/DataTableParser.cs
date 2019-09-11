using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Ecoset.GeoTemporal.Remote
{
    public class DataTableParser : IParser<DataTableListResult>
    {
        public DataTableListResult TryParse(string raw)
        {
            var parsed = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(raw);
            var model = new DataTableListResult();
            model.Rows = parsed;
            return model;
        }
    }

    public class DataTableStatsParser : IParser<DataTableStatsResult>
    {
        public DataTableStatsResult TryParse(string raw)
        {
            var parsed = (RawEcosetSummaryData)JsonConvert.DeserializeObject(raw, typeof(RawEcosetSummaryData));
            var result = new DataTableStatsResult() 
            {
                Stats = parsed.Stats
            };
            return result;
        }

        private class RawEcosetSummaryData
        {
            [JsonProperty("summary")]
            public Statistics Stats { get; set; }
        }
    }

    public class CsvParser : IParser<CsvFileResult>
    {
        public CsvFileResult TryParse(string raw)
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