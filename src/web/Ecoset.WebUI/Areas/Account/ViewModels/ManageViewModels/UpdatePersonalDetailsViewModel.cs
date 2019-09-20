using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models.ManageViewModels
{
    public class UpdatePersonalDetailsViewModel
    {
        [Required]
        [Display(Name = "Organisation Name")]
        public string OrganisationName { get; set; }
        [Required]
        [Display(Name = "Organisation Type")]
        public OrganisationType OrganisationType { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        public string Surname { get; set; }
    }
}
