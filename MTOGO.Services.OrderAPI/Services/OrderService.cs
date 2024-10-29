using Dapper;
using Microsoft.Data.SqlClient;
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
            _cartRequestQueue = configuration["TopicAndQueueNames:CartRequestQueue"];
            _cartResponseQueue = configuration["TopicAndQueueNames:CartResponseQueue"];
        }

        public async Task<int> CreateOrderAsync(OrderDto order)
        {
            try
            {
                var correlationId = Guid.NewGuid();
                var cartRequest = new CartRequestMessage
                {
                    UserId = order.UserId,
                    CorrelationId = correlationId
                };

                // Publish the cart request to the queue
                await _messageBus.PublishMessage(_cartRequestQueue, JsonConvert.SerializeObject(cartRequest));

                // Wait for the cart response
                var cartResponse = await WaitForCartResponseAsync(correlationId);

                // Populate order with cart data
                order.Items = cartResponse.Items;
                order.TotalAmount = cartResponse.Items.Sum(item => item.Price * item.Quantity);
                order.VATAmount = order.TotalAmount * 0.2m;

                return await SaveOrderAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new order");
                throw;
            }
        }

        private async Task<CartResponseMessage> WaitForCartResponseAsync(Guid correlationId)
        {
            var tcs = new TaskCompletionSource<CartResponseMessage>();
            _logger.LogInformation($"Subscribing to {_cartResponseQueue} with CorrelationId: {correlationId}");

            _messageBus.SubscribeMessage<CartResponseMessage>(_cartResponseQueue, message =>
            {
                _logger.LogInformation($"Received message with CorrelationId: {message.CorrelationId}");
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



        private async Task<int> SaveOrderAsync(OrderDto order)
        {
            try
            {
                var orderItemsTable = new DataTable();
                orderItemsTable.Columns.Add("MenuItemId", typeof(int));
                orderItemsTable.Columns.Add("Price", typeof(decimal));
                orderItemsTable.Columns.Add("Quantity", typeof(int));

                foreach (var item in order.Items)
                {
                    orderItemsTable.Rows.Add(item.MenuItemId, item.Price, item.Quantity);
                }

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", order.UserId);
                parameters.Add("@RestaurantId", order.RestaurantId);
                parameters.Add("@DeliveryAgentId", order.DeliveryAgentId);
                parameters.Add("@TotalAmount", order.TotalAmount);
                parameters.Add("@VATAmount", order.VATAmount);
                parameters.Add("@OrderPlacedTimestamp", DateTime.UtcNow); // Auto-set timestamp
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

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
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

        public async Task<int> UpdateOrderStatusAsync(int orderId, int statusId)
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

        public async Task<int> DeleteOrderAsync(int id)
        {
            try
            {
                var sql = @"
                    DELETE FROM OrderItem WHERE OrderId = @OrderId;
                    DELETE FROM [Order] WHERE Id = @OrderId;";
                return await _dataAccess.Delete(sql, new { OrderId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order with ID {id}");
                throw;
            }
        }
    }
}
