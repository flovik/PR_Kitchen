using Kitchen.Models;

namespace Kitchen.Interfaces
{
    public interface IKitchenService
    {
        public Task ReceiveOrder(Order order);
        public Task SendReturnOrder();
    }
}
