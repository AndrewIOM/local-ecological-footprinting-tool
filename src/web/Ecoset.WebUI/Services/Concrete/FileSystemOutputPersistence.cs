using Ecoset.WebUI.Services.Abstract;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Ecoset.WebUI.Models;
using System.Linq;

namespace Ecoset.WebUI.Services.Concrete
{
    public class FileSystemOutputPersistence : IOutputPersistence
    {
        private ILogger<FileSystemOutputPersistence> _logger;
        private IOptions<FileSystemPersistenceOptions> _options;
        private IDataFormatter _formatter;

        public FileSystemOutputPersistence(
            ILogger<FileSystemOutputPersistence> logger, 
            IOptions<FileSystemPersistenceOptions> options,
            IDataFormatter formatter) {
            _logger = logger;
            _options = options;
            _formatter = formatter;
        }

        public string PersistProData(int jobId, List<ProDataItem> items)
        {
            var fileName = _options.Value.PersistenceFolder + jobId + "_data.zip";
            if (File.Exists(fileName)) {
                _logger.LogWarning("Overwriting file with new pro data: " + fileName);
                File.Delete(fileName);
            }
            var zipFile = ZipFile.Open(fileName, ZipArchiveMode.Create);

            foreach(var item in items)
            {
                if (item.Contents == null || item.LayerName == null || item.FileExtension == null) continue;
                var entry = zipFile.CreateEntry(item.LayerName + "." + item.FileExtension, CompressionLevel.Optimal);
                using (BinaryWriter writer = new BinaryWriter(entry.Open()))
                {
                    if (item.FileExtension == "csv") {
                        byte[] textbyte = Encoding.Unicode.GetBytes(item.Contents);
                        writer.Write(textbyte);
                    } else if (item.FileExtension == "tif") {
                        byte[] byteArray = Convert.FromBase64String(item.Contents); 
                        writer.Write(byteArray,0,byteArray.Length);
                    }
                }
            }
            zipFile.Dispose();
            return fileName;
        }

        public string PersistReport(int jobId, string temporaryFile)
        {
            var fileName = Path.Combine(_options.Value.PersistenceFolder, jobId + "_report.pdf");
            if (File.Exists(fileName)) _logger.LogInformation("Overwriting previous report for job #" + jobId);
            if (!File.Exists(temporaryFile)) {
                _logger.LogError("The temporary cache file for job #" + jobId + " did not exist. Perhaps there was a problem with its generation?");
                return "";
            }
            File.Copy(temporaryFile, fileName, true);
            return fileName;
        }

        private static string StringToCSVCell(string str)
        {
            bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }

        public string PersistData(int jobId, ReportData data)
        {
            var fileName = _options.Value.PersistenceFolder + jobId + "_data.zip";
            if (File.Exists(fileName)) {
                _logger.LogWarning("Overwriting file with new pro data: " + fileName);
                File.Delete(fileName);
            }
            var zipFile = ZipFile.Open(fileName, ZipArchiveMode.Create);

            foreach (var item in data.RawResults)
            {
                var entry = zipFile.CreateEntry(item.Name + ".tif", CompressionLevel.Optimal);
                using (BinaryWriter writer = new BinaryWriter(entry.Open())) {
                    var extension = _formatter.SpatialData(item.Data, writer.BaseStream);
                }
            }

            foreach (var item in data.TableListResults) {
                var entry = zipFile.CreateEntry(item.Name + ".csv", CompressionLevel.Optimal);
                using (var writer = new BinaryWriter(entry.Open())) {
                    var uniqueRowNames = item.Data.Rows.Select(r => r.Keys.Select(k => k.ToString())).SelectMany(x => x);
                    writer.Write(String.Concat(uniqueRowNames.Select(StringToCSVCell), ','));
                    writer.Write('\n');
                    foreach (var record in item.Data.Rows) {
                        var processed = uniqueRowNames.Select(n => {
                            if (record.ContainsKey(n)) {
                                return record[n];
                            } else return "";
                        });
                        writer.Write(String.Concat(processed.Select(StringToCSVCell), ','));
                        writer.Write('\n');
                    }
                }
            }

            zipFile.Dispose();
            return fileName;
        }

        public string GetProData(int jobId)
        {
            var fileName = Path.Combine(_options.Value.PersistenceFolder, jobId + "_data.zip");
            if (!File.Exists(fileName)) return "";
            return fileName;
        }

        public string GetReport(int jobId)
        {
            var fileName = Path.Combine(_options.Value.PersistenceFolder, jobId + "_report.pdf");
            if (!File.Exists(fileName)) return "";
            return fileName;
        }

        public static void DeSerialize(string fileName, string serializedFile)
        {
            using (System.IO.FileStream reader = System.IO.File.Create(fileName))
            {
                byte[] buffer = Convert.FromBase64String(serializedFile); 
                reader.Write(buffer, 0, buffer.Length);
            }
        }

    }
}
