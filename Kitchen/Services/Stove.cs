using Kitchen.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Xml.Linq;

namespace Kitchen.Services
{
    public class Stove
    {
        public ApparatusState State { get; set; } = ApparatusState.Free;
        public Thread Thread;
        public int TimeUnit { get; }
        public Mutex PreparingOrdersMutex { get; set; }
        public ConcurrentDictionary<int, ReturnOrder> PreparingOrders { get; set; }
        public ILogger<Stove> _logger { get; set; }

        public Stove(int timeUnit)
        {
            TimeUnit = timeUnit;
        }

        public Task Prep(int CookId, (int, Food) food)
        {



            return Task.CompletedTask;
        }

        public void PrepFood(int CookId, (int, Food) food)
        {
            Thread.Sleep(food.Item2.PreparationTime * TimeUnit);

            //add in preparing orders the cooking details
            PreparingOrdersMutex.WaitOne();
            PreparingOrders[food.Item1].CookingDetails.Add(new CookingDetails
            {
                CookId = CookId,
                FoodId = food.Item2.Id,
            });
            PreparingOrdersMutex.ReleaseMutex();

            _logger.LogWarning($"Stove made food {food.Item2.Id} from order {food.Item1}");
        }
    }
}
