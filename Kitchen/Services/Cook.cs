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

        public Cook(int cookId, int proficiency, string name, string catchPhrase, int rank)
        {
            CookId = cookId;
            Proficiency = proficiency;
            ProficiencyThreads = new List<Thread>(proficiency);
            Name = name;
            CatchPhrase = catchPhrase;
            Rank = rank;
        }

        public void Prep((int, Food) food)
        {
            //take any thread that is not alive (meaning waiting to prepare something)
            for (int i = 0; i < ProficiencyThreads.Count; i++)
            {
                if (ProficiencyThreads[i].IsAlive) continue;

                //thread is used, lower proficiency
                Proficiency--;
                ProficiencyThreads[i] = new Thread(() => PrepFood(food));
                ProficiencyThreads[i].Start();
            }
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

            //thread is released, higher proficiency
            Proficiency++;
        }

        public bool CanCook(int complexity)
        {
            return Rank >= complexity && Proficiency != 0;
        }
    }
}
