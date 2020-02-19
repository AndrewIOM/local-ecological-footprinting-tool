using System;
using System.Linq;
using System.Threading.Tasks;
using Ecoset.GeoTemporal.Remote;
using Ecoset.WebUI.Services.Abstract;
using BitMiracle.LibTiff.Classic;
using System.Collections.Generic;

namespace Ecoset.WebUI.Services.Concrete
{
    public class TiffDataFormatter : IDataFormatter
    {

        public string SpatialData(RawDataResult spatialData, System.IO.Stream saveLocation) {

            var noDataValue = -9999; // Pull this through from ecoset...
            var maxValue = spatialData.DataCube.Cast<double>().Select(m => m != noDataValue).Max();
            var minValue = spatialData.DataCube.Cast<double>().Select(m => m != noDataValue).Min();
            Console.WriteLine("[Making TIFF] Max = " + maxValue + " and Min = " + minValue);

            Tiff.SetTagExtender(TagExtender);
            using (Tiff output = Tiff.ClientOpen("InMemory", "w", null, new TiffStream()))
            {
                var height = spatialData.DataCube.GetLength(0);
                var width = spatialData.DataCube.GetLength(1);

                output.SetField(TiffTag.IMAGEWIDTH, height);
                output.SetField(TiffTag.IMAGELENGTH, width);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                output.SetField(TiffTag.BITSPERSAMPLE, 16);
                output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                output.SetField(TiffTag.ROWSPERSTRIP, height);

                for (int i = 0; i < height; i++)
                {
                    var rowData = DataCubeHelpers.SliceRow(spatialData.DataCube, i).ToArray();
                    short[] samples = new short[width];
                    for (int j = 0; j < width; j++) {
                        samples[j] = (short)rowData[j];   // DODGY CAST!
                    }
                    byte[] buffer = new byte[samples.Length * sizeof(short)];
                    Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                    output.WriteScanline(buffer, i);
                }

                // int numberOfDirectories = input.NumberOfDirectories();
                // for (short i = 0; i < numberOfDirectories; ++i)
                // {
                //     input.SetDirectory(i);

                //     copyTags(input, output);
                //     copyStrips(input, output);

                //     output.WriteDirectory();
                // }
            }
            return ".tif";
        }

        public string TableData(DataTableListResult tableData, System.IO.Stream saveLocation) {
            throw new NotImplementedException();
        }

        private void TagExtender(Tiff tiff)
        {
            TiffFieldInfo[] tiffFieldInfo = 
            {
                new TiffFieldInfo(TiffTag.GEOTIFF_MODELTIEPOINTTAG, 6, 6, TiffType.DOUBLE, FieldBit.Custom, false, true, "MODELTILEPOINTTAG"),
                new TiffFieldInfo(TiffTag.GEOTIFF_MODELPIXELSCALETAG, 3, 3, TiffType.DOUBLE, FieldBit.Custom, false, true, "MODELPIXELSCALETAG")
            };
            tiff.MergeFieldInfo(tiffFieldInfo, tiffFieldInfo.Length);
        }

    }

    public static class DataCubeHelpers {

        public static IEnumerable<T> SliceRow<T>(this T[,] array, int row)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                yield return array[i, row];
            }
        }

    }
}