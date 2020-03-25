using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ecoset.GeoTemporal.Remote
{
    /// Polymorphic converter for three key ecoset data structures into .net types.
    /// See: https://blog.maartenballiauw.be/post/2020/01/29/deserializing-json-into-polymorphic-classes-with-systemtextjson.html
    public class DataFieldTypeConverter : JsonConverter<IDataResult>
    {
        public override bool CanConvert(Type objectType) => typeof(IDataResult).IsAssignableFrom(objectType);

        public override IDataResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Check for null values
            if (reader.TokenType == JsonTokenType.Null) return null;

            // Copy the current state from reader
            var readerAtStart = reader;

            // Figure out which ecoset result type is present
            using var jsonDocument = JsonDocument.ParseValue(ref reader);
            var jsonObject = jsonDocument.RootElement;

            // 1. Raster data result
            if (jsonObject.TryGetProperty("data", out var raw)) {
                var parsed = JsonSerializer.Deserialize<RawEcosetData>(ref readerAtStart, options);
                return new RawDataResult() 
                {
                    Stats = parsed.Stats,
                    DataCube = To2D(parsed.Data.Data, parsed.Data.Columns, parsed.Data.Rows),
                    Columns = parsed.Data.Columns,
                    Rows = parsed.Data.Rows
                } as IDataResult;
            }

            // 2. Summary result
            if (jsonObject.TryGetProperty("summary", out var stats)) {
                var parsed = JsonSerializer.Deserialize<RawEcosetSummaryData>(ref readerAtStart, options);
                return new DataTableStatsResult() {
                    Stats = parsed.Stats
                } as IDataResult;
            }

            // 3. Data table result
            if (jsonObject.ValueKind == JsonValueKind.Array) {
                var list = JsonSerializer.Deserialize<List<Dictionary<string,string>>>(ref readerAtStart, options);
                return new DataTableListResult() {
                    Rows = list
                } as IDataResult;
            }

            // 4. Unknown result type not yet implemented
            throw new NotSupportedException("Ecoset data result type can not be deserialized");
        }

        public override void Write(Utf8JsonWriter writer, IDataResult value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

        private double?[,] To2D(double?[][] inputRaster, int cols, int rows) {
            var dest = new double?[rows,cols];
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    dest[i,j] = inputRaster[i][j];
                }
            }
            return dest;
        }

        private class RawEcosetSummaryData
        {
            [JsonPropertyName("summary")]
            public Statistics Stats { get; set; }
        }

        private class RawEcosetData
        {
            [JsonPropertyName("summary")]
            public Statistics Stats { get; set; }
            [JsonPropertyName("data")]
            public RawDataset Data { get; set; }
        }

        private class RawDataset
        {
            [JsonPropertyName("raw")]
            public double?[][] Data { get; set; }
            [JsonPropertyName("ncols")]
            public int Columns { get; set; }
            [JsonPropertyName("nrows")]
            public int Rows { get; set; }
        }

    }
}