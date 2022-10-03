using Kitchen.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Xml.Linq;

namespace Kitchen.Services
{
    public class Oven
    {
        public int Id { get; set; }      
        public ApparatusState State { get; set; } = ApparatusState.Free;
        public Thread Thread { get; set; }
        public int TimeUnit { get; }
        public Mutex PreparingOrdersMutex { get; set; }
        public ConcurrentDictionary<int, ReturnOrder> PreparingOrders { get; set; }
        public ILogger<Oven> _logger { get; set; }

        public Oven(int id, int timeUnit)
        {
            Id = id;
            TimeUnit = timeUnit;
        }

        public Task Prep(int CookId, (int, Food) food)
        {
            Thread = new Thread(() => PrepFood(CookId, food));
            Thread.Start();
            return Task.CompletedTask;
        }

        public void PrepFood(int CookId, (int, Food) food)
        {
            _logger.LogCritical($"An oven {Id} is used by cook {CookId} making " +
                                $"{food.Item2.Id} from order {food.Item1}");
            Thread.Sleep(food.Item2.PreparationTime * TimeUnit);

            //add in preparing orders the cooking details
            PreparingOrdersMutex.WaitOne();
            PreparingOrders[food.Item1].CookingDetails.Add(new CookingDetails
            {
                CookId = CookId,
                FoodId = food.Item2.Id,
            });
            PreparingOrdersMutex.ReleaseMutex();

            _logger.LogCritical($"Oven made food {food.Item2.Id} from order {food.Item1}");
            State = ApparatusState.Free;
        }
    }
}
