using Kitchen.Models;
using System.Collections.Concurrent;

namespace Kitchen.Services
{
    public class Cook
    {
        public int CookId { get; }
        public int Rank { get; }
        public string Name { get; }
        public string CatchPhrase { get; }
        public int TimeUnit { get; set; }
        public List<Thread> ProficiencyThreads;
        public int Proficiency { get; private set; }
        public Mutex PreparingOrdersMutex { get; set; }
        public ConcurrentDictionary<int, ReturnOrder> PreparingOrders { get; set; }
        public ILogger<Cook> _logger { get; set; }

        public Cook(int cookId, int proficiency, string name, string catchPhrase, int rank)
        {
            CookId = cookId;
            Proficiency = proficiency;
            ProficiencyThreads = new List<Thread>();
            for (int i = 0; i < proficiency; i++)
            {
                ProficiencyThreads.Add(new Thread(() => {}));
            }
            Name = name;
            CatchPhrase = catchPhrase;
            Rank = rank;
        }

        public Task Prep((int, Food) food)
        {
            //take any thread that is not alive (meaning waiting to prepare something)
            for (int i = 0; i < ProficiencyThreads.Count; i++)
            {
                if (ProficiencyThreads[i].IsAlive) continue;

                //thread is used, lower proficiency
                Proficiency--;
                ProficiencyThreads[i] = new Thread(() => PrepFood(food))
                {
                    Name = $"{CookId} | {i}"
                };

                ProficiencyThreads[i].Start();
                break;
            }

            return Task.CompletedTask;
        }

        public void PrepFood((int, Food) food)
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

            _logger.LogWarning($"Cook {Name} made food {food.Item2.Id} from order {food.Item1}");

            //thread is released, higher proficiency
            Proficiency++;
        }

        public Task UseStove(Stove stove, (int, Food) food)
        {
            Task.Run(() => stove.Prep(CookId, food));
            return Task.CompletedTask;
        }

        public Task UseOven(Oven oven, (int, Food) food)
        {
            Task.Run(() => oven.Prep(CookId, food));
            return Task.CompletedTask;
        }

        public bool CanCook(int complexity)
        {
            if (Rank < complexity || Proficiency == 0) return false;
            return true;
        }
    }
}
