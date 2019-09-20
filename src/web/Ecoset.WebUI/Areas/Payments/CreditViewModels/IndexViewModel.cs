using System;
using System.Collections.Generic;

namespace Ecoset.WebUI.Models.CreditViewModels {
    public class IndexViewModel
    {
        public int CurrentCredits {get;set;}
        public List<PurchaseSummaryViewModel> Purchases {get;set;}
    }

    public class PurchaseSummaryViewModel {
        public int Id {get;set;}
        public DateTime Time {get;set;}
        public decimal TotalSum {get;set;}
        public int Quantity {get;set;}
        public string Description {get;set;}
    }
}