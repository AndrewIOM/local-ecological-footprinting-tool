namespace Ecoset.WebUI.Options 
{
    public class EcosetAppOptions {
        public string InstanceName {get; set; }
        public string InstanceShortName {get; set; }
        public string GoogleAnalyticsId { get; set; }
        public string Organisation { get; set; }
        public string MapboxAccessToken { get; set; }
        public string MapboxStaticKey { get; set; }
        public int VimeoPromotionVideoId { get; set; }
        public bool SampleReportEnabled { get; set; }
        public bool PaymentsEnabled { get; set; }
        public double MaximumAnalysisHeight { get; set; }
        public double MaximumAnalysisWidth { get; set; }

        public EcosetAppOptions() {
            InstanceName = "Ecoset";
            InstanceShortName = "Ecoset";
            GoogleAnalyticsId = null;
            Organisation = "Ecoset Provider";
            MapboxAccessToken = null;
            MapboxStaticKey = null;
            VimeoPromotionVideoId = 0;
            SampleReportEnabled = true;
            PaymentsEnabled = false;
            MaximumAnalysisHeight = 4.00;
            MaximumAnalysisWidth = 4.00;
        }
    }
}