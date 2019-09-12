
namespace Ecoset.WebUI.Models.JobProcessor {
    public class JobSubmission {
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public double North { get; set; }
        public double South { get; set; }
        public double East { get; set; }
        public double West { get; set; }
        public int Priority { get; set; }
    }
}