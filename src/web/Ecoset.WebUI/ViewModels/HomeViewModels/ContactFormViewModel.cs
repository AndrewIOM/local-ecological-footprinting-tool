using System.ComponentModel.DataAnnotations;

namespace Oxlel.Left.WebUI.Models.HomeViewModels {
    public class ContactFormViewModel
    {
        [Required]
        [Display(Name = "Your Name")]
        public string Name {get;set;}
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email {get;set;}
        [Required]
        [EmailAddress]
        [Display(Name = "Confirm Email Address")]
        [Compare("Email", ErrorMessage = "The email addresses do not match.")]
        public string ConfirmEmail {get;set;}
        [Required]
        [Display(Name = "Please enter your message")]
        public string Message {get;set;}
    }
}