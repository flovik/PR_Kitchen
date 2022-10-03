using Kitchen.Interfaces;
using Kitchen.Models;
using Kitchen.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Kitchen.Controllers
{
    [Route("api")]
    [ApiController]
    public class KitchenController : ControllerBase
    {
        private readonly ILogger<KitchenController> _logger;
        private readonly IKitchenService _kitchenService;
        public KitchenController(ILogger<KitchenController> logger, IKitchenService kitchenService)
        {
            _logger = logger;
            _kitchenService = kitchenService;
        }

        [HttpPost("order")]
        public IActionResult Order([FromBody] Order order)
        {
            //endpoint for kitchen server, here comes returnOrder
            _logger.LogWarning($"Dining Hall sent order with ID {order.OrderId} " +
                                                    $"from table {order.TableId} " +
                                                    $"by waiter {order.WaiterId}");

            _logger.LogWarning(JsonConvert.SerializeObject(order));

            //add order in _orderQueue
            _kitchenService.AddToOrder(order);

            return NoContent();
        }
    }
}
