using System;
using System.Collections.Generic;
using Ecoset.WebUI.Enums;
using Ecoset.WebUI.Models;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface IOutputPersistence
    {
        string PersistProData(int jobId, List<ProDataItem> items);
        string PersistData(int jobId, ReportData data);
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