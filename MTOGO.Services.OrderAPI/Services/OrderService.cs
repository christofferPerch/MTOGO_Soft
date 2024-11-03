using Dapper;
using MTOGO.MessageBus;
using MTOGO.Services.DataAccess;
using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Services.IServices;
using Newtonsoft.Json;
using System.Data;

namespace MTOGO.Services.OrderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<OrderService> _logger;
        private readonly IMessageBus _messageBus;
        private readonly string _cartRequestQueue;
        private readonly string _cartResponseQueue;

        public OrderService(IDataAccess dataAccess, ILogger<OrderService> logger, IMessageBus messageBus, IConfiguration configuration)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _messageBus = messageBus;

            _cartRequestQueue = configuration.GetValue<string>("TopicAndQueueNames:CartRequestQueue");
            _cartResponseQueue = configuration.GetValue<string>("TopicAndQueueNames:CartResponseQueue");

            if (string.IsNullOrEmpty(_cartRequestQueue) || string.IsNullOrEmpty(_cartResponseQueue))
            {
                throw new Exception("Queue names are not configured properly in appsettings.json.");
            }
        }

        public async Task<int> CreateOrder(OrderDto order)
        {
            try
            {
                var correlationId = Guid.NewGuid();
                var cartRequest = new CartRequestMessage
                {
                    UserId = order.UserId,
                    CorrelationId = correlationId
                };

                await _messageBus.PublishMessage(_cartRequestQueue, JsonConvert.SerializeObject(cartRequest));

                var cartResponse = await WaitForCartResponse(correlationId);

                order.Items = cartResponse.Items;
                order.TotalAmount = cartResponse.Items.Sum(item => item.Price * item.Quantity);
                order.VATAmount = order.TotalAmount * 0.2m;

                return await SaveOrder(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new order");
                throw;
            }
        }

        private async Task<CartResponseMessage> WaitForCartResponse(Guid correlationId)
        {
            var tcs = new TaskCompletionSource<CartResponseMessage>();

            _messageBus.SubscribeMessage<CartResponseMessage>("CartResponseQueue", message =>
            {
                if (message.CorrelationId == correlationId)
                {
                    _logger.LogInformation("CorrelationId matched. Setting result in TaskCompletionSource.");
                    tcs.SetResult(message);
                }
            });

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(45)));
            if (completedTask == tcs.Task)
            {
                return tcs.Task.Result;
            }
            else
            {
                throw new TimeoutException("Timeout waiting for CartResponse message.");
            }
        }

        private async Task<int> SaveOrder(OrderDto order)
        {
            try
            {
                var orderItemsTable = new DataTable();
                orderItemsTable.Columns.Add("MenuItemId", typeof(int));
                orderItemsTable.Columns.Add("Price", typeof(decimal));
                orderItemsTable.Columns.Add("Quantity", typeof(int));

                foreach (var item in order.Items)
                {
                    orderItemsTable.Rows.Add(
                        item.MenuItemId,
                        item.Price,
                        item.Quantity
                    );
                }

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", order.UserId);
                parameters.Add("@RestaurantId", order.RestaurantId);
                parameters.Add("@DeliveryAgentId", order.DeliveryAgentId);
                parameters.Add("@TotalAmount", order.TotalAmount);
                parameters.Add("@VATAmount", order.VATAmount);
                parameters.Add("@OrderPlacedTimestamp", DateTime.UtcNow);
                parameters.Add("@OrderStatusId", (int)OrderStatus.FreeToTake);
                parameters.Add("@OrderItems", orderItemsTable.AsTableValuedParameter("TVP_OrderItem"));
                parameters.Add("@OrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _dataAccess.ExecuteStoredProcedure<int>("AddOrder", parameters);

                return parameters.Get<int>("@OrderId");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving order to the database.");
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderById(int id)
        {
            try
            {
                var orderSql = "SELECT * FROM [Order] WHERE Id = @Id;";
                var order = await _dataAccess.GetById<OrderDto>(orderSql, new { Id = id });

                if (order == null)
                {
                    return null;
                }

                var orderItemsSql = "SELECT * FROM OrderItem WHERE OrderId = @OrderId;";
                var orderItems = await _dataAccess.GetAll<OrderItemDto>(orderItemsSql, new { OrderId = id });
                order.Items = orderItems;

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving order with ID {id}");
                throw;
            }
        }

        public async Task<int> UpdateOrderStatus(int orderId, int statusId)
        {
            try
            {
                var sql = "UPDATE [Order] SET OrderStatusId = @StatusId WHERE Id = @OrderId;";
                return await _dataAccess.Update(sql, new { OrderId = orderId, StatusId = statusId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order status for ID {orderId}");
                throw;
            }
        }
    }
}
