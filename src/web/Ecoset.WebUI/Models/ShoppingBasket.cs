using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecoset.WebUI.Models {
    public class ShoppingBasket 
    {
        [Key]
        public int Id { get; set; }
        public int Credits { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal TotalCostIncludingVat { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal PerUnitCost { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool TransactionCompleted { get; set; }
    }
}