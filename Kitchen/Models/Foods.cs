namespace Kitchen.Models
{
    public class Foods
    {
        //id-to-food dictionary
        public readonly IDictionary<int, Food> foods = new Dictionary<int, Food>()
        {
            {1, new Food()
            {
                Id = 1,
                Name = "pizza",
                PreparationTime = 20,
                Complexity = 2,
                CookingApparatus = CookingApparatus.Oven
            } },
            {2, new Food()
            {
                Id = 2,
                Name = "salad",
                PreparationTime = 10,
                Complexity = 1,
                CookingApparatus = CookingApparatus.Null
            } },
            {3, new Food()
            {
                Id = 3,
                Name = "zeama",
                PreparationTime = 7,
                Complexity = 1,
                CookingApparatus = CookingApparatus.Stove
            } },
            {4, new Food()
            {
                Id = 4,
                Name = "Scallop Sashimi with Meyer Lemon Confit",
                PreparationTime = 32,
                Complexity = 3,
                CookingApparatus = CookingApparatus.Null
            } },
            {5, new Food()
            {
                Id = 5,
                Name = "Island Duck with Mulberry Mustard",
                PreparationTime = 35,
                Complexity = 3,
                CookingApparatus = CookingApparatus.Oven
            } },
            {6, new Food()
            {
                Id = 6,
                Name = "Waffles",
                PreparationTime = 10,
                Complexity = 1,
                CookingApparatus = CookingApparatus.Stove
            } },
            {7, new Food()
            {
                Id = 7,
                Name = "Aubergine",
                PreparationTime = 20,
                Complexity = 2,
                CookingApparatus = CookingApparatus.Oven
            } },
            {8, new Food()
            {
                Id = 8,
                Name = "Lasagna",
                PreparationTime = 30,
                Complexity = 2,
                CookingApparatus = CookingApparatus.Oven
            } },
            {9, new Food()
            {
                Id = 9,
                Name = "Burger",
                PreparationTime = 15,
                Complexity = 1,
                CookingApparatus = CookingApparatus.Stove
            } },
            {10, new Food()
            {
                Id = 10,
                Name = "Gyros",
                PreparationTime = 15,
                Complexity = 1,
                CookingApparatus = CookingApparatus.Null
            } },
            {11, new Food()
            {
                Id = 11,
                Name = "Kebab",
                PreparationTime = 15,
                Complexity = 1,
                CookingApparatus = CookingApparatus.Null
            } },
            {12, new Food()
            {
                Id = 12,
                Name = "Unagi Maki",
                PreparationTime = 20,
                Complexity = 2,
                CookingApparatus = CookingApparatus.Null
            } },
            {13, new Food()
            {
                Id = 13,
                Name = "Tobacco Chicken",
                PreparationTime = 30,
                Complexity = 2,
                CookingApparatus = CookingApparatus.Oven
            } },
        };
    }
}
