using System.Collections.Generic;

namespace Ecoset.WebUI.Options
{
    public class ReportContentOptions
    {
        public int? MaxMapPixelSize { get; set; }
        public int? MapPixelSize { get; set; }
        public List<ReportSection> FreeReportSections { get; set; }
        public List<ReportSection> ProReportSections { get; set; }
    }

    public class ReportSection
    {
        public string Name { get; set; }
        public string Method { get; set; }
        public Dictionary<string,string> Options { get; set; }

        public ReportSection() {
            Options = new Dictionary<string,string>();
        }
    }
}