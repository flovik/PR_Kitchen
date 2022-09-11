namespace Kitchen.Services
{
    public class Cook
    {
        public int CookId { get; set; }
        public int Proficiency { get; set; }
        public string Name { get; set; }
        public string CatchPhrase { get; set; }

        public Cook(int cookId, int proficiency, string name, string catchPhrase)
        {
            CookId = cookId;
            Proficiency = proficiency;
            Name = name;
            CatchPhrase = catchPhrase;
        }


    }
}
