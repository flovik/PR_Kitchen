using Kitchen.Interfaces;
using Kitchen.Models;
using RestSharp;

namespace Kitchen.Services
{
    public class KitchenSender : IKitchenSender
    {
        private static RestClient _client = new();
        private readonly ILogger<KitchenSender> _logger;

        public KitchenSender(ILogger<KitchenSender> logger)
        {
            _logger = logger;
            _client = new RestClient("http://host.docker.internal:8090/");

        }

        public void SendReturnOrder(ReturnOrder returnOrder)
        {
            _logger.LogWarning($"Sending back order {returnOrder.OrderId}");

            var request = new RestRequest("api/distribution").AddJsonBody(returnOrder);
            _client.Post(request);
        }
    }
}
