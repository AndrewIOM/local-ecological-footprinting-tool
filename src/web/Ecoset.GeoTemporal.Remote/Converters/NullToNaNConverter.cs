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
            var str = reader.GetString();
            if (str == null) {
                return System.Single.NaN;
            } else {
                return System.Single.Parse(str);
            }
        }

        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

    }
}