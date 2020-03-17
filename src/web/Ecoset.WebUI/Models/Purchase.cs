using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecoset.WebUI.Models {
    public class Purchase {
        public int Id {get;set;}
        public DateTime Time {get;set;}
        public ApplicationUser PurchasedBy { get; set; }

        //Order Details
        [Column(TypeName = "decimal(6,2)")]
        public decimal Total {get; set; }
        public int NumberOfCredits {get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal UnitPrice {get; set; }
        public string CurrencyCode {get; set; }

        //Billing Details
        public string Name {get;set;}
        public string AddressLine1 {get;set;}
        public string AddressLine2 {get;set;}
        public string PostCode {get;set;}
        public string Country {get;set;}
        public string SettlementCurrency {get; set; }
    }
}