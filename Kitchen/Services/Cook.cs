using Kitchen.Models;

namespace Kitchen.Services
{
    public class Cook
    {
        public int CookId { get; }
        public int Rank { get; }
        public int Proficiency { get; }
        public string Name { get; }
        public string CatchPhrase { get; }
        public int TimeUnit { get; set; }

        public Cook(int cookId, int proficiency, string name, string catchPhrase, int rank)
        {
            CookId = cookId;
            Proficiency = proficiency;
            Name = name;
            CatchPhrase = catchPhrase;
            Rank = rank;
        }

        public CookingDetails PrepFood(Food food)
        {
            Thread.Sleep(food.PreparationTime * TimeUnit);

            return new CookingDetails
            {
                CookId = CookId,
                FoodId = food.Id,
            };
        }
    }
}
