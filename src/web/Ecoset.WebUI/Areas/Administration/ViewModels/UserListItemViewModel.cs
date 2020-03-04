
using System;

namespace Ecoset.WebUI.Models.AdminViewModels {
    public class UserListItemViewModel
    {
        public DateTime RegistrationDate { get; set; }
        public string Id {get;set;}
        public string UserName {get;set;}
        public string Name { get; set; }
        public string Organisation { get; set; }
        public int ActiveJobs {get;set;}
        public int CompleteJobs {get;set;}
        public bool IsAdmin { get; set; }
        public int Credits { get; set; }
    }

    public class UserDropdownItemViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}