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
        public string Method { get; set; }
        public object Options { get; set; }
    }
}