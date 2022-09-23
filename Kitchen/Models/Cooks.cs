using Kitchen.Services;

namespace Kitchen.Models
{
    public static class Cooks
    {
        public static IDictionary<int, Cook> cooks = new Dictionary<int, Cook>
        {
            {
                1, new Cook(
                    cookId: 1,
                    rank: 3,
                    proficiency: 4,
                    name: "Gordon Ramsay",
                    catchPhrase: "Bollocks")
            },
            {
                2, new Cook(
                    cookId: 2,
                    rank: 2,
                    proficiency: 3,
                    name: "Petea",
                    catchPhrase: "Eu sunt Petea, hai sa bem")
            },
            {
                3, new Cook(
                    cookId: 3,
                    rank: 2,
                    proficiency: 2,
                    name: "Grisa",
                    catchPhrase: "Eu cu Petea nu mai beu, dupa dansu nia ni-i greu")
            },
            {
                4, new Cook(
                    cookId: 4,
                    rank: 1,
                    proficiency: 2,
                    name: "Borea",
                    catchPhrase: "Azi ii luni patani, care bem...")
            }
        };
    }
}
