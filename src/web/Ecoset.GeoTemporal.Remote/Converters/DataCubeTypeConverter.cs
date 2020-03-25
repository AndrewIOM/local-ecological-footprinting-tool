using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ecoset.GeoTemporal.Remote
{
    public class DataCubeTypeConverter : JsonConverter<double?[,]>
    {
        public override double?[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var arr = JsonSerializer.Deserialize<double?[][]>(ref reader, options);
            var rows = arr.Count();
            var cols = arr.First().Count();
            return To2D(arr, cols, rows);            
        }

        public override void Write(Utf8JsonWriter writer, double?[,] value, JsonSerializerOptions options)
        {
            var multi = ToMulti(value);
            JsonSerializer.Serialize(writer, multi, multi.GetType(), options);
        }

        private double?[][] ToMulti(double?[,] arr) {
            var nrow = arr.GetLength(0);
            var ncol = arr.GetLength(1);
            var res = new double?[nrow][];
            for (int r = 0; r < nrow; r++) {
                res[r] = new double?[ncol];
                for (int c = 0; c < ncol; c++) {
                    res[r][c] = arr[r,c];
                }
            }
            return res;
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

    }
}