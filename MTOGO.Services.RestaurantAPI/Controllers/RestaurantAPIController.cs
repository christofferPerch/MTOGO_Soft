using Microsoft.AspNetCore.Mvc;
using MTOGO.MessageBus;
using MTOGO.Services.RestaurantAPI.Models;
using MTOGO.Services.RestaurantAPI.Models.Dto;
using MTOGO.Services.RestaurantAPI.Services.IServices;

namespace MTOGO.Services.RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/restaurant")]
    public class RestaurantAPIController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RestaurantAPIController> _logger;
        protected ResponseDto _response;

        #region Constructor
        public RestaurantAPIController(IRestaurantService restaurantService, IMessageBus messageBus,
            IConfiguration configuration, ILogger<RestaurantAPIController> logger)
        {
            _restaurantService = restaurantService;
            _messageBus = messageBus;
            _configuration = configuration;
            _logger = logger;
            _response = new();
        }
        #endregion

        #region Post Methods
        [HttpPost("add")]
        public async Task<IActionResult> AddRestaurant([FromBody] RestaurantDto restaurant)
        {
            _logger.LogInformation("Received request to add a new restaurant.");

            try
            {
                if (restaurant == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant data is invalid.";
                    return BadRequest(_response);
                }

                var restaurantId = await _restaurantService.AddRestaurant(restaurant);
                _response.Result = restaurantId;
                _response.Message = "Restaurant added successfully.";

                var message = $"New restaurant added: {restaurant.Name} with ID: {restaurantId}";
                await _messageBus.PublishMessage(_configuration.GetValue<string>("TopicAndQueueNames:RestaurantAddedQueue"), message);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in AddRestaurant method.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while adding the restaurant.";
                return StatusCode(500, _response);
            }
        }

        [HttpPost("addMenuItem")]
        public async Task<IActionResult> AddMenuItem([FromBody] AddMenuItemDto menuItemDto)
        {
            _logger.LogInformation("Received request to add a menu item.");
            try
            {
                if (menuItemDto == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Menu item data is invalid.";
                    return BadRequest(_response);
                }

                var menuItemId = await _restaurantService.AddMenuItem(menuItemDto);
                if (menuItemId == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "An error occurred while adding the menu item.";
                    return StatusCode(500, _response);
                }

                _response.Result = menuItemId;
                _response.Message = "Menu item added successfully.";

                var message = $"New menu item added: {menuItemDto.Name} for Restaurant ID: {menuItemDto.RestaurantId}";
                await _messageBus.PublishMessage(_configuration.GetValue<string>("TopicAndQueueNames:MenuItemAddedQueue"), message);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the menu item.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while adding the menu item.";
                return StatusCode(500, _response);
            }
        }
        #endregion

        #region Put Methods
        [HttpPut("updateRestaurant")]
        public async Task<IActionResult> UpdateRestaurant([FromBody] UpdateRestaurantDto updateRestaurantDto)
        {
            _logger.LogInformation("Received request to update restaurant.");
            try
            {
                if (updateRestaurantDto == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant data is invalid.";
                    return BadRequest(_response);
                }

                var result = await _restaurantService.UpdateRestaurant(updateRestaurantDto);
                if (result == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant not found.";
                    return NotFound(_response);
                }

                _response.Message = "Restaurant updated successfully.";

                var message = $"Restaurant updated: {updateRestaurantDto.Name} with ID: {updateRestaurantDto.Id}";
                await _messageBus.PublishMessage(_configuration.GetValue<string>("TopicAndQueueNames:RestaurantUpdatedQueue"), message);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the restaurant.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while updating the restaurant.";
                return StatusCode(500, _response);
            }
        }
        #endregion

        #region Delete Methods
        [HttpDelete("deleteSpecificMenuItem")]
        public async Task<IActionResult> RemoveMenuItem(int restaurantId, int menuItemId)
        {
            _logger.LogInformation($"Received request to delete menu item with ID: {menuItemId}");
            try
            {
                var result = await _restaurantService.RemoveMenuItem(restaurantId, menuItemId);
                if (result == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Menu item not found.";
                    return NotFound(_response);
                }

                _response.Message = "Menu item removed successfully.";

                var message = $"Menu item with ID: {menuItemId} has been removed.";
                await _messageBus.PublishMessage(_configuration.GetValue<string>("TopicAndQueueNames:MenuItemRemovedQueue"), message);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing the menu item.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while removing the menu item.";
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("deleteSpecificRestaurant")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            _logger.LogInformation($"Received request to delete restaurant with ID: {id}");
            try
            {
                var result = await _restaurantService.DeleteRestaurant(id);
                if (result == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant not found.";
                    return NotFound(_response);
                }

                _response.Message = "Restaurant deleted successfully.";

                var message = $"Restaurant with ID: {id} has been deleted.";
                await _messageBus.PublishMessage(_configuration.GetValue<string>("TopicAndQueueNames:RestaurantDeletedQueue"), message);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the restaurant.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while deleting the restaurant.";
                return StatusCode(500, _response);
            }
        }
        #endregion

        #region Get Methods
        [HttpGet("getSpecificRestaurant")]
        public async Task<IActionResult> GetRestaurantById(int id)
        {
            _logger.LogInformation($"Received request to retrieve restaurant with ID: {id}");
            try
            {
                var restaurant = await _restaurantService.GetRestaurantById(id);
                if (restaurant == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant not found.";
                    return NotFound(_response);
                }

                _response.Result = restaurant;
                _response.Message = "Restaurant retrieved successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the restaurant.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving the restaurant.";
                return StatusCode(500, _response);
            }
        }

        [HttpGet("allRestaurants")]
        public async Task<IActionResult> GetAllRestaurants()
        {
            _logger.LogInformation("Received request to retrieve all restaurants.");
            try
            {
                var restaurants = await _restaurantService.GetAllRestaurants();
                _response.Result = restaurants;
                _response.Message = "All restaurants retrieved successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all restaurants.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving all restaurants.";
                return StatusCode(500, _response);
            }
        }
        #endregion
    }
}
