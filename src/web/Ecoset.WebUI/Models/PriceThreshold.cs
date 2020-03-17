
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecoset.WebUI.Models {
    public class PriceThreshold {
        public int Id { get; set; }
        public int Units { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal UnitCost { get; set; }
    }
}