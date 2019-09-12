using System;
using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models {
    public class ShoppingBasket 
    {
        [Key]
        public int Id { get; set; }
        public int Credits { get; set; }
        public decimal TotalCostIncludingVat { get; set; }
        public decimal PerUnitCost { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool TransactionCompleted { get; set; }
    }
}