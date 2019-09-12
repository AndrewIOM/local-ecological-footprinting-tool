using System.Threading.Tasks;
using Ecoset.WebUI.Models;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface IPaymentService
    {
        Task<PaymentResponse> MakePayment(PaymentRequest request);
    }
}
