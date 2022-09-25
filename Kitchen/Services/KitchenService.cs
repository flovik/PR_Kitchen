using System.Collections.Concurrent;
using Kitchen.Interfaces;
using Kitchen.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kitchen.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly ConcurrentDictionary<int, Order> _orderList; //list of orders
        private readonly ConcurrentDictionary<int, ReturnOrder> _preparingOrders = new(); //orders that are currently prepared, find order by its ID
        private readonly ILogger<KitchenService> _logger;
        private readonly BlockingCollection<Cook> _cooks = new();
        private readonly ConcurrentDictionary<int, ConcurrentQueue<(int, Food)>> _foodList; //dictionary that saves foods based on their complexity in queues
        private static readonly Mutex OrderListMutex = new();
        private static readonly Mutex PreparingOrdersMutex = new();
        private readonly IKitchenSender _kitchenSender;
        private readonly SemaphoreSlim _foodListSemaphore;
        private readonly SemaphoreSlim _stovesSemaphore;
        private readonly SemaphoreSlim _ovensSemaphore;
        public int TimeUnit { get; }

        public KitchenService(IConfiguration configuration, ILogger<KitchenService> logger, IKitchenSender kitchenSender)
        {
            _logger = logger;
            _kitchenSender = kitchenSender;
            _orderList = new ConcurrentDictionary<int, Order>();

            //init dictionary with 3 types of complexity
            _foodList = new ConcurrentDictionary<int, ConcurrentQueue<(int, Food)>>
            {
                [1] = new(), //complexity 1
                [2] = new(), //complexity 2 
                [3] = new(), //complexity 3
                [4] = new(), //stoves row
                [5] = new() //ovens row
            };

            //init time unit
            TimeUnit = configuration.GetValue<int>("TIME_UNIT");

            //init stoves and ovens
            var stoves = configuration.GetValue<int>("Stoves");
            var ovens = configuration.GetValue<int>("Ovens");
            _stovesSemaphore = new SemaphoreSlim(stoves, stoves);
            _ovensSemaphore = new SemaphoreSlim(ovens, ovens);

            //initialize cooks
            var cooks = configuration.GetValue<int>("Cooks");

            for (int i = 0; i < cooks; i++)
            {
                //for each proficiency of a cook, add an instance of that cook that can prepare a specific food
                var cook = Cooks.cooks[i + 1];
                for (int j = 0; j < cook.Proficiency; j++)
                {
                    _cooks.Add(cook);
                }
            }

            //grant access to food for every instance of cook, all cooks + one thread that adds the food, stoves and ovens
            _foodListSemaphore = new SemaphoreSlim(_cooks.Count + stoves + ovens + 1, _cooks.Count + stoves + ovens + 1);

            //start threading with cooks
            foreach (var cook in _cooks)
            {
                //set the time unit for each waiter before he begins working
                cook.TimeUnit = TimeUnit;
                Task.Run(() => Start(cook));
            }
        }

        public void Start(Cook cook)
        {
            while (true)
            {
                //cooking apparatus will execute here, because it is not considered food that occupies a whole thread
                //a specific waiter can take the food and put in the stove

                //run new task here
                CheckApparatusFood(cook, 4); //for stoves
                CheckApparatusFood(cook, 5); //for ovens

                _foodListSemaphore.Wait();
                //iterate every foodQueue (from 1 to 3) the cook can cook from highest to lowest
                for (int complexity = 3; complexity >= 1; complexity--)
                {
                    //if cook cannot cook that food, skip it
                    if (cook.Rank < complexity) continue;

                    //if no foods, don't cook
                    if (_foodList[complexity].IsEmpty) continue;

                    //remove food from foodList
                    var isAnyFood = _foodList[complexity].TryDequeue(out var result);

                    if(!isAnyFood) break;

                    PrepareFood(cook, result);
                }

                _foodListSemaphore.Release();
            }
        }

        public void CheckApparatusFood(Cook cook, int row)
        {
            _foodListSemaphore.Wait();
            if (!_foodList[row].IsEmpty) //no cooking apparatus foods
            {
                var isFood = _foodList[row].TryDequeue(out var food);
                if (isFood && cook.Rank >= food.Item2.Complexity) //check if could get stove food and that current cook's complexity is enough
                {
                    switch (row)
                    {
                        case 4:
                            _stovesSemaphore.Wait();
                            _logger.LogCritical($"A stove is used! Remaining: {_stovesSemaphore.CurrentCount}");
                            Task.Run(() => PrepareFood(cook, food)).Wait(); //new thread for preparing food in stove, wait for it when done
                            //then release it for another food to be cooked
                            _stovesSemaphore.Release();
                            _logger.LogCritical($"A stove is released! Remaining: {_stovesSemaphore.CurrentCount}");
                            break;
                        case 5:
                            _ovensSemaphore.Wait();
                            _logger.LogCritical($"An oven is used! Remaining: {_ovensSemaphore.CurrentCount}");
                            Task.Run(() => PrepareFood(cook, food)).Wait();
                            _ovensSemaphore.Release();
                            _logger.LogCritical($"An oven is released! Remaining: {_ovensSemaphore.CurrentCount}");
                            break;
                    }
                    
                }
            }
            _foodListSemaphore.Release();
        }

        public void PrepareFood(Cook cook, (int, Food) food)
        {
            _logger.LogWarning($"Food {food.Item2.Id} from order {food.Item1} " +
                               $"is being prepared by cook {cook.Name}");

            var cookingDetails = Task.Run(() => cook.PrepFood(food.Item2)).Result;

            PreparingOrdersMutex.WaitOne();
            //add in preparing orders the cooking details
            _preparingOrders[food.Item1].CookingDetails.Add(cookingDetails);
            _logger.LogWarning($"New cooking details have been added to order {food.Item1} by {cook.Name}");
            PreparingOrdersMutex.ReleaseMutex();

            //call function after adding cooking details
            //checks if returnOrder is ready to be dispatched
            CheckIfReady();
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
                    //time when last cooking details were added - pickup time of order
                    returnOrder.CookingTime = (int) (((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() - _preparingOrders[orderId].PickUpTime);
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

            _foodListSemaphore.Wait();
            //add foods in the priority dictionary
            foreach (var foodId in foodList)
            {
                //find corresponding food
                var food = Foods.foods[foodId];

                //add stoves/oven
                switch (food.CookingApparatus)
                {
                    case CookingApparatus.Stove:
                        _foodList[4].Enqueue((order.OrderId, food));
                        break;
                    case CookingApparatus.Oven:
                        _foodList[5].Enqueue((order.OrderId, food));
                        break;
                    default:
                        //add in foodList based on complexity food and its order id
                        _foodList[food.Complexity].Enqueue((order.OrderId, food));
                        //_logger.LogWarning($"Food {food.Id} of order {order.OrderId} " +
                        //    $"has been added to the foodList");
                        break;
                }

                
            }
            _foodListSemaphore.Release();
        }
    }
}
