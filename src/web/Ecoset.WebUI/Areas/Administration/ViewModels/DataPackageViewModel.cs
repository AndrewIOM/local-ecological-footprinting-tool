
using System;

namespace Ecoset.WebUI.Models.AdminViewModels {
    public class DataPackageViewModel
    {
        public Guid Id {get;set;}
        public DateTime DateAdded {get;set;}
        public DateTime? DateCompleted {get;set;}
        public string SubmittedBy {get;set;}
        public string Status {get;set;}
        public string Query { get; set; }
    }
}