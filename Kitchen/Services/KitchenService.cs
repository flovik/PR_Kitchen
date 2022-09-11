using System.Text;
using Kitchen.Interfaces;
using Kitchen.Models;
using Newtonsoft.Json;
using RestSharp;

namespace Kitchen.Services
{
    public class KitchenService : IKitchenService
    {
        private static RestClient _client;
        private Order order;
        private readonly ILogger<KitchenService> _logger;
        public KitchenService(IConfiguration Configuration, ILogger<KitchenService> logger)
        {
            _logger = logger;
            _client = new RestClient("http://host.docker.internal:8090/");
        }

        public async Task ReceiveOrder(Order order)
        {
            _logger.LogInformation($"Came order {order.OrderId} from " +
                                   $"{order.TableId}");
            this.order = order;
            await SendReturnOrder();
        }

        public async Task SendReturnOrder()
        {
            var returnOrder = new ReturnOrder(order, 50, new List<CookingDetails>
            {
                new CookingDetails
                {
                    CookId = 1,
                    FoodId = 1
                }
            });

            _logger.LogInformation($"Sending back order");
            
            var request = new RestRequest("api/distribution").AddJsonBody(returnOrder);
            var response = await _client.PostAsync(request);
            ;
        }
    }
}
