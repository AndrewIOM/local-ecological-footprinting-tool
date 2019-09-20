using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models.CreditViewModels {
    public class CreditBasketItemViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Credits {get;set;}
        public double VatRate { get; set; }
        public decimal PerUnitCostExcludingVat {get; set;}
        public decimal OrderTotalExcludingVat { get; set; }
        public decimal OrderTotalIncludingVat {get; set;}
    }
}