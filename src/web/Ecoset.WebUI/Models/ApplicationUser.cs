using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Ecoset.WebUI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string OrganisationName { get; set; }
        public OrganisationType OrganisationType { get; set; }

        public List<Job> Jobs { get; set; }
        public int Credits { get; set; }
        public List<Purchase> Purchases { get; set; }
        public List<Notification> Notifications { get; set; }

        public DateTime RegistrationDate { get; set; }

        public bool AcceptedTerms { get; set; }
        public bool AgreedToCommunication { get; set; }
        public bool EmailOnJobCompletion { get; set; }

        public virtual List<Subscription> Subscriptions { get; set; }

        /// <summary>
        /// Navigation property for the roles this user belongs to.
        /// </summary>
        public virtual ICollection<IdentityUserRole<string>> Roles { get; } = new List<IdentityUserRole<string>>();
    }

    public enum OrganisationType {
        Commercial,
        NonCommercial
    }
}
