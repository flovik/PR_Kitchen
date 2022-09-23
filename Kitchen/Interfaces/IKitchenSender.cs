using Kitchen.Models;

namespace Kitchen.Interfaces;

public interface IKitchenSender
{
    public void SendReturnOrder(ReturnOrder returnOrder);
}