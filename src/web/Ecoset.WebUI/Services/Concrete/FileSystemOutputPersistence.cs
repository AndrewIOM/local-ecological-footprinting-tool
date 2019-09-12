using Ecoset.WebUI.Services.Abstract;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace Ecoset.WebUI.Services.Concrete
{
    public class FileSystemOutputPersistence : IOutputPersistence
    {
        private ILogger<FileSystemOutputPersistence> _logger;
        private IOptions<FileSystemPersistenceOptions> _options;

        public FileSystemOutputPersistence(ILogger<FileSystemOutputPersistence> logger, IOptions<FileSystemPersistenceOptions> options) {
            _logger = logger;
            _options = options;
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
            var fileName = _options.Value.PersistenceFolder + jobId + "_report.pdf";
            if (File.Exists(fileName)) _logger.LogInformation("Overwriting previous report for job #" + jobId);
            if (!File.Exists(temporaryFile)) {
                _logger.LogError("The temporary cache file for job #" + jobId + " did not exist. Perhaps there was a problem with its generation?");
                return "";
            }
            File.Copy(temporaryFile, fileName, true);
            return fileName;
        }

        public string GetProData(int jobId)
        {
            var fileName = _options.Value.PersistenceFolder + jobId + "_data.zip";
            if (!File.Exists(fileName)) return "";
            return fileName;
        }

        public string GetReport(int jobId)
        {
            var fileName = _options.Value.PersistenceFolder + jobId + "_report.pdf";
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
