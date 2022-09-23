using System.Collections.Concurrent;
using Kitchen.Interfaces;
using Kitchen.Models;

namespace Kitchen.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly ConcurrentDictionary<int, Order> _orderList; //list of orders
        private ConcurrentDictionary<int, ReturnOrder> _preparingOrders = new(); //orders that are currently prepared, find order by its ID
        private readonly ILogger<KitchenService> _logger;
        private BlockingCollection<Cook> _cooks = new();
        private ConcurrentDictionary<int, ConcurrentQueue<(int, Food)>> _foodList; //dictionary that saves foods based on their complexity in queues
        private static Mutex OrderListMutex = new();
        private static Mutex PreparingOrdersMutex = new();
        private static Mutex FoodListMutex = new();
        private readonly IKitchenSender _kitchenSender;
        private readonly SemaphoreSlim FoodListSemaphore;

        public KitchenService(IConfiguration configuration, ILogger<KitchenService> logger, IKitchenSender kitchenSender)
        {
            _logger = logger;
            _kitchenSender = kitchenSender;
            _orderList = new ConcurrentDictionary<int, Order>();

            //init dictionary with 3 types of complexity
            _foodList = new ConcurrentDictionary<int, ConcurrentQueue<(int, Food)>>
            {
                [1] = new ConcurrentQueue<(int, Food)>(),
                [2] = new ConcurrentQueue<(int, Food)>(),
                [3] = new ConcurrentQueue<(int, Food)>()
            };

            //initialize cooks
            var cooks = configuration.GetValue<int>("Cooks");
            FoodListSemaphore = new SemaphoreSlim(cooks + 1, cooks + 1);

            for (int i = 0; i < cooks; i++)
            {
                _cooks.Add(Cooks.cooks[i + 1]);
            }

            //start threading with cooks
            foreach (var cook in _cooks)
            {
                Task.Run(() => Start(cook));
            }
        }

        //TODO add from order list to food list and keep somehow track

        public void Start(Cook cook)
        {
            //TODO process food list
            while (true)
            {
                //FoodListMutex.WaitOne();
                FoodListSemaphore.Wait();
                //iterate every foodQueue the cook can cook from highest to lowest
                for (int complexity = 3; complexity >= 1; complexity--)
                {
                    //if cook cannot cook that food, skip it
                    if (cook.Rank < complexity) continue;

                    //for each proficiency of a cook, give him a food to cook
                    //a cook will take a food if he can by his proficiency or if food list has any foods
                    while (cook.CanCook != 0 && !_foodList[complexity].IsEmpty)
                    {
                        //remove food from foodList
                        var isAnyFood = _foodList[complexity].TryDequeue(out var result);

                        if(!isAnyFood) break;

                        _logger.LogWarning($"Food {result.Item2.Id} from order {result.Item1} " +
                            $"is being prepared by cook {cook.Name}");
                        var cookingDetails = Task.Run(() => cook.PrepFood(result.Item2)).Result;
                        
                        PreparingOrdersMutex.WaitOne();
                        //add in preparing orders the cooking details
                        _preparingOrders[result.Item1].CookingDetails.Add(cookingDetails);
                        _logger.LogWarning($"New cooking details have been added to order {result.Item1}");
                        PreparingOrdersMutex.ReleaseMutex();

                        //call function after adding cooking details
                        // checks if returnOrder is ready to be dispatched
                        CheckIfReady();
                    }
                }

                //FoodListMutex.ReleaseMutex();
                FoodListSemaphore.Release();

            }
        }

        public void CheckIfReady()
        {
            PreparingOrdersMutex.WaitOne();
            //for every preparing order check if it is ready to dispatch
            foreach (var (orderId, returnOrder) in _preparingOrders.ToList())
            {
                //check number of cooking details with the total number of items in the original order
                if (returnOrder.CookingDetails.Count == _orderList[orderId].Items.Count)
                {
                    returnOrder.CookingTime = 1;
                    _logger.LogWarning($"Order {returnOrder.OrderId} is ready to be returned back");
                    OrderListMutex.WaitOne();
                    //delete order from _orderList
                    _orderList.TryRemove(new KeyValuePair<int, Order>(orderId, _orderList[orderId]));
                    OrderListMutex.ReleaseMutex();

                    //send return order to hall
                    _logger.LogWarning($"Order {returnOrder.OrderId} is dispatched to Dining Hall!");
                    _kitchenSender.SendReturnOrder(returnOrder);

                    //delete returnOrder from _preparingOrders
                    _preparingOrders.TryRemove(new KeyValuePair<int, ReturnOrder>(orderId, returnOrder));
                }
            }

            PreparingOrdersMutex.ReleaseMutex();
        }

        public void AddToOrder(Order order)
        {
            OrderListMutex.WaitOne();
            _orderList[order.OrderId] = order;
            _logger.LogWarning($"Order {order.OrderId} has been added to the Order List!");
            OrderListMutex.ReleaseMutex();
            BreakDownOrderToFoodList(order); 
        }

        public void BreakDownOrderToFoodList(Order order)
        {
            PreparingOrdersMutex.WaitOne();
            //add order to dictionary of preparing orders
            _preparingOrders[order.OrderId] = new ReturnOrder(order, 1, new List<CookingDetails>());
            _logger.LogWarning($"Order {order.OrderId} has been added to the preparing orders");
            var foodList = order.Items;
            PreparingOrdersMutex.ReleaseMutex();

            FoodListSemaphore.Wait();
            //FoodListMutex.WaitOne();
            //add foods in the priority dictionary
            foreach (var foodId in foodList)
            {
                //find corresponding food
                var food = Foods.foods[foodId];

                //add in foodList based on complexity food and its order id
                _foodList[food.Complexity].Enqueue((order.OrderId, food));
                _logger.LogWarning($"Food {food.Id} of order {order.OrderId} " +
                    $"has been added to the foodList");
            }
            //FoodListMutex.ReleaseMutex();
            FoodListSemaphore.Release();
        }
    }
}
