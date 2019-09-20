using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
