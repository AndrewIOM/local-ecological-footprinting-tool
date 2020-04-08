using System.Collections.Generic;
using Ecoset.WebUI.Services.Abstract;

namespace Ecoset.WebUI.Models.DashboardViewModels {
    public class IndexViewModel
    {
        public string UserName { get; set; }
        public int CreditCount { get; set; }
        public ActiveSubscription Subscription { get; set; }
        public int DataPackageCount { get; set; }
    }

    public class ApiUseViewModel
    {
        public List<DataPackage> DataPackages { get; set; }
        public ActiveSubscription Subscription { get; set; }
    }
}