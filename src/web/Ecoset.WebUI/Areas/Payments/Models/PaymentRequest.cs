
namespace Ecoset.WebUI.Models {
    public class PaymentRequest {
        public decimal Amount {get;set;}
        public string Description {get;set;}
        public string ThirdPartyHash {get;set;}
    }
}