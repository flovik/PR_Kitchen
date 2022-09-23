using Kitchen.Models;

namespace Kitchen.Services
{
    public class Cook
    {
        public int CookId { get; set; }
        public int Rank { get; set; }
        public int Proficiency { get; set; }
        public int CanCook { get; set; }
        public string Name { get; set; }
        public string CatchPhrase { get; set; }

        public Cook(int cookId, int proficiency, string name, string catchPhrase, int rank)
        {
            CookId = cookId;
            Proficiency = proficiency;
            Name = name;
            CatchPhrase = catchPhrase;
            Rank = rank;
            CanCook = proficiency;
        }

        public CookingDetails PrepFood(Food food)
        {
            //TODO logic of preparing food
            Thread.Sleep(food.PreparationTime * 1000);

            //CanCook++;
            return new CookingDetails
            {
                CookId = CookId,
                FoodId = food.Id,
            };
        }
    }
}
