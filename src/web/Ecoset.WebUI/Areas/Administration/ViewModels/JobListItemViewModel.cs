
using System;

namespace Ecoset.WebUI.Models.AdminViewModels {
    public class JobListItemViewModel
    {
        public int Id {get;set;}
        public DateTime DateAdded {get;set;}
        public DateTime? DateCompleted {get;set;}
        public string Name {get;set;}
        public string Description {get;set;}
        public string SubmittedBy {get;set;}
        public string Status {get;set;}
        public string ProStatus {get;set;}
    }
}