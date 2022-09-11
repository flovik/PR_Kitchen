namespace Kitchen.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public int WaiterId { get; set; }
        public ICollection<int> Items { get; set; } = new List<int>();
        public int Priority { get; set; }
        public int MaxWait { get; set; }
        public long PickUpTime { get; set; } = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();

        protected Order(Order thisOrder)
        {
            OrderId = thisOrder.OrderId;
            TableId = thisOrder.TableId;
            WaiterId = thisOrder.WaiterId;
            Items = thisOrder.Items;
            Priority = thisOrder.Priority;
            MaxWait = thisOrder.MaxWait;
            PickUpTime = thisOrder.PickUpTime;
        }

        public Order()
        {
        }
    }
}
