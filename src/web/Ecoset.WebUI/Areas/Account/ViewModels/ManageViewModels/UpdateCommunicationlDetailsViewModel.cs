using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models.ManageViewModels
{
    public class UpdateCommunicationDetailsViewModel
    {
        [Display(Name = "I agree to receive emails for general information and marketing purposes.")]
        public bool AgreedToCommunication { get; set; }

        [Display(Name = "I agree to receive emails when my analyses are ready.")]
        public bool EmailOnJobCompletion { get; set; }
    }
}
