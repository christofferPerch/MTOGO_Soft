using Dapper;
using Microsoft.Data.SqlClient;
using MTOGO.Services.DataAccess;
using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Services.IServices;
using System.Data;

namespace MTOGO.Services.OrderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IDataAccess dataAccess, ILogger<OrderService> logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public async Task<int> CreateOrderAsync(OrderDto order)
        {
            try
            {
                var orderItemsTable = new DataTable();
                orderItemsTable.Columns.Add("MenuItemId", typeof(int));
                orderItemsTable.Columns.Add("MenuItemName", typeof(string));
                orderItemsTable.Columns.Add("Price", typeof(decimal));
                orderItemsTable.Columns.Add("Quantity", typeof(int));

                foreach (var item in order.Items)
                {
                    orderItemsTable.Rows.Add(item.MenuItemId, item.MenuItemName, item.Price, item.Quantity);
                }

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", order.UserId);
                parameters.Add("@RestaurantId", order.RestaurantId);
                parameters.Add("@DeliveryAgentId", order.DeliveryAgentId);
                parameters.Add("@TotalAmount", order.TotalAmount);
                parameters.Add("@VATAmount", order.VATAmount);
                parameters.Add("@OrderPlacedTimestamp", order.OrderPlacedTimestamp);
                parameters.Add("@OrderStatusId", order.OrderStatusId);
                parameters.Add("@OrderItems", orderItemsTable.AsTableValuedParameter("TVP_OrderItem"));
                parameters.Add("@OrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _dataAccess.ExecuteStoredProcedure<int>("AddOrder", parameters);
                return parameters.Get<int>("@OrderId");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new order");
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
