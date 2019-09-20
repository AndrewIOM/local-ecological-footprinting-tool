using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Required]
        [Display(Name = "Organisation Name")]
        public string OrganisationName { get; set; }

        [Required]
        [Display(Name = "Organisation Type")]
        public OrganisationType OrganisationType { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "By ticking this box I agree with the terms and conditions.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms and conditions.")]
        public bool TermsAndConditions { get; set; }

        [Display(Name = "I agree to receive emails for general information and marketing purposes.")]
        public bool AgreedToCommunication { get; set; }

        [Display(Name = "I agree to receive emails when my analyses are ready.")]
        public bool EmailOnJobCompletion { get; set; }
    }
}
