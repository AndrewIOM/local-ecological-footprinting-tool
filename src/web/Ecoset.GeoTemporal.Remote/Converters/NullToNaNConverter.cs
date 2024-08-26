using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ecoset.GeoTemporal.Remote
{
    public class NullToNaNConverter : JsonConverter<float>
    {
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            reader.Read();
            if (reader.TokenType == JsonTokenType.Null)
            {
                return System.Single.NaN;
            }
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException();
            }
            return reader.GetSingle();
        }

        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}