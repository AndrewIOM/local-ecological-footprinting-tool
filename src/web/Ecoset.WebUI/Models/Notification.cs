using System;
using System.ComponentModel.DataAnnotations;
using Ecoset.WebUI.Enums;

namespace Ecoset.WebUI.Models {
    public class Notification 
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Time { get; set; }
        public NotificationLevel Level { get; set; }
        public string Text { get; set; }
        public string TextValues { get; set; }
        public Job Job { get; set; }
        public ApplicationUser User { get; set; }
        public bool Hidden { get; set; }
    }
}