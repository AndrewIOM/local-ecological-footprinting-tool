using System.Collections.Generic;

namespace Ecoset.WebUI.Options
{
    public class ReportContentOptions
    {
        public List<ReportSection> FreeReportSections { get; set; }
        public List<ReportSection> ProReportSections { get; set; }
    }

    public class ReportSection
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string OutputFormat { get; set; }
        public string Stat { get; set; }
    }
}