using Microsoft.AspNetCore.Mvc;
using MTOGO.MessageBus;
using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Services.IServices;

namespace MTOGO.Services.OrderAPI.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderAPIController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<OrderAPIController> _logger;
        protected ResponseDto _response;

        public OrderAPIController(IOrderService orderService, IMessageBus messageBus, ILogger<OrderAPIController> logger)
        {
            _orderService = orderService;
            _messageBus = messageBus;
            _logger = logger;
            _response = new();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto order)
        {
            _logger.LogInformation("Received request to create a new order.");
            try
            {
                if (order == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Order data is invalid.";
                    return BadRequest(_response);
                }

                var orderId = await _orderService.CreateOrderAsync(order);
                _response.Result = orderId;
                _response.Message = "Order created successfully.";

                await _messageBus.PublishMessage("OrderCreatedQueue", $"Order {orderId} created for user {order.UserId}");
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while creating the order.";
                return StatusCode(500, _response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            _logger.LogInformation($"Received request to retrieve order with ID: {id}");
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Order not found.";
                    return NotFound(_response);
                }

                _response.Result = order;
                _response.Message = "Order retrieved successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving the order.";
                return StatusCode(500, _response);
            }
        }

        [HttpPut("updateStatus/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] int statusId)
        {
            _logger.LogInformation($"Received request to update status for order ID: {orderId}");
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(orderId, statusId);
                if (success == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Order not found.";
                    return NotFound(_response);
                }

                _response.Message = "Order status updated successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while updating order status.";
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            _logger.LogInformation($"Received request to delete order with ID: {id}");
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (result == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Order not found.";
                    return NotFound(_response);
                }

                _response.Message = "Order deleted successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while deleting the order.";
                return StatusCode(500, _response);
            }
        }
    }
}
