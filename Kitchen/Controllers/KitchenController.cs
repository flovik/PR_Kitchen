using Kitchen.Interfaces;
using Kitchen.Models;
using Kitchen.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Order([FromBody] Order order)
        {
            //endpoint for kitchen server, here comes returnOrder
            _logger.Log(LogLevel.Information, 1000, $"Dining Hall sent order with ID {order.OrderId} " +
                                                    $"from table {order.TableId}");

            await _kitchenService.ReceiveOrder(order);

            return NoContent();
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {

            return Ok("heell");
        }
    }
}
