using System;
using System.Linq;
using Ecoset.GeoTemporal.Remote;
using Ecoset.WebUI.Services.Abstract;
using BitMiracle.LibTiff.Classic;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;

namespace Ecoset.WebUI.Services.Concrete
{
    public class TiffDataFormatter : IDataFormatter
    {
        private EcosetAppOptions _options;
        public TiffDataFormatter(IOptions<EcosetAppOptions> options) {
            _options = options.Value;
        }

        public string SpatialData(RawDataResult spatialData) {

            var maxValue = spatialData.DataCube.Cast<double>().Where(m => m != spatialData.NoDataValue).Max();
            var minValue = spatialData.DataCube.Cast<double>().Where(m => m != spatialData.NoDataValue).Min();
            Console.WriteLine("[Making TIFF] Max = " + maxValue + " and Min = " + minValue);

            Tiff.SetTagExtender(TagExtender);
            var tempFile = System.IO.Path.Combine(_options.ScratchDirectory, Guid.NewGuid().ToString() + ".tif");
            using (var output = Tiff.Open(tempFile, "w"))
            {
                var height = spatialData.DataCube.GetLength(0);
                var width = spatialData.DataCube.GetLength(1);

                output.SetField(TiffTag.IMAGEWIDTH, width);
                output.SetField(TiffTag.IMAGELENGTH, height);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                output.SetField(TiffTag.SOFTWARE, _options.InstanceShortName.ToUpper().Replace(" ", ""));
                output.SetField(TiffTag.ROWSPERSTRIP, height);

                if (minValue >= 0 && maxValue <= 255) {
                    Console.WriteLine("Writing 8-bit unsigned raster");
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.BITSPERSAMPLE, 8);
                    for (int i = 0; i < height; i++) {
                        var rowData = DataCubeHelpers.SliceRow(spatialData.DataCube, i).ToArray();
                        byte[] buffer = new byte[width];
                        for (int j = 0; j < width; j++) {
                            buffer[j] = (byte)rowData[j];
                        }
                        output.WriteScanline(buffer, i);
                    }
                } else {
                    Console.WriteLine("Writing 16-bit signed raster");
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.BITSPERSAMPLE, 16);
                    for (int i = 0; i < height; i++)
                    {
                        var rowData = DataCubeHelpers.SliceRow(spatialData.DataCube, i).ToArray();
                        short[] samples = new short[width];
                        for (int j = 0; j < width; j++) {
                            samples[j] = (short)rowData[j];
                        }
                        byte[] buffer = new byte[samples.Length * sizeof(short)];
                        Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                        output.WriteScanline(buffer, i);
                    }
                }

            }
            return tempFile;
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
            for (var i = 0; i < array.GetLength(1); i++)
            {
                yield return array[row, i];
            }
        }

    }
}