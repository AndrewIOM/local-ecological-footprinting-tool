using System;
using System.Collections.Generic;
using Ecoset.WebUI.Enums;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface IOutputPersistence
    {
        string PersistProData(int jobId, List<ProDataItem> items);
        string PersistReport(int jobId, string temporaryFile);
        string GetProData(int jobId);
        string GetReport(int jobId);
    }

    public class ProDataItem 
    {
        public string LayerName { get; set; }
        public string FileExtension { get; set; }
        public string Contents { get; set; }
    }
}