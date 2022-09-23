namespace Kitchen.Models
{
    public class ReturnOrder : Order
    {
        public int CookingTime { get; set; }
        public ICollection<CookingDetails> CookingDetails { get; set; }

        public ReturnOrder(Order thisOrder, int cookingTime, ICollection<CookingDetails> cookingDetails) : base(thisOrder)
        {
            CookingTime = cookingTime;
            CookingDetails = cookingDetails;
        }

        public ReturnOrder()
        {

        }
    }
}
