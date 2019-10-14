using Ecoset.WebUI.Services.Abstract;
using Ecoset.WebUI.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;

namespace Ecoset.WebUI.Services.Concrete
{
    public class WorldPayPaymentService : IPaymentService
    {
        private PaymentOptions _options;
        public WorldPayPaymentService(IOptions<PaymentOptions> payOptions) {
            _options = payOptions.Value;
        }

        public async Task<PaymentResponse> MakePayment(PaymentRequest request)
        {
            var webrequest = (HttpWebRequest)WebRequest.CreateHttp(_options.WorldPayUrl);
            webrequest.Method = "POST";
            webrequest.ContentType = "application/json";
            webrequest.Headers["Authorization"] = _options.WorldPayServerAuthToken;

            var worldPayRequest = new WorldPayRequest() {
                token = request.ThirdPartyHash,
                orderDescription = request.Description,
                amount = decimal.ToInt32(request.Amount * 100), //Pounds to pence
                currencyCode = "GBP",
                settlementCurrency = "GBP"
            };

            string json = JsonConvert.SerializeObject(worldPayRequest);
            var requestStream = await webrequest.GetRequestStreamAsync();
            using (var streamWriter = new StreamWriter(requestStream))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var httpResponse = await webrequest.GetResponseAsync();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var jsonResult = streamReader.ReadToEnd();
                var result = (WorldPayResponse)JsonConvert.DeserializeObject(jsonResult, typeof(WorldPayResponse));


                var response = new PaymentResponse();
                response.Success = result.PaymentStatus == "SUCCESS";
                return response;
            }
        }

        private class WorldPayRequest {
            public string token {get;set;}
            public int amount {get;set;}
            public string currencyCode {get;set;}
            public string orderDescription {get;set;}
            public string settlementCurrency {get;set;}
        }

        private class WorldPayResponse {
            public string OrderCode {get;set;}
            public string Token {get;set;}
            public string PaymentStatus {get;set;}
            public string Amount {get;set;}
        }
    }
}
