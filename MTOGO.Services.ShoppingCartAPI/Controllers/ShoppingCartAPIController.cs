using Microsoft.AspNetCore.Mvc;
using MTOGO.MessageBus;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;
using MTOGO.Services.ShoppingCartAPI.Services.IServices;

namespace MTOGO.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/shoppingcart")]
    public class ShoppingCartAPIController : ControllerBase
    {
        private readonly IShoppingCartService _cartService;
        private readonly IMessageBus _messageBus;
        protected ResponseDto _response;

        public ShoppingCartAPIController(IShoppingCartService cartService, IMessageBus messageBus)
        {
            _cartService = cartService;
            _messageBus = messageBus;
            _response = new ResponseDto();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            try
            {
                var cart = await _cartService.GetCart(userId);
                if (cart == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Cart not found.";
                    return NotFound(_response);
                }

                _response.Result = cart;
                _response.Message = "Cart retrieved successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving the cart." + ex;
                return StatusCode(500, _response);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCart([FromBody] Cart cart)
        {
            try
            {
                var createdCart = await _cartService.CreateCart(cart);
                _response.Result = createdCart;
                _response.Message = "Cart created successfully.";
                return CreatedAtAction(nameof(GetCart), new { userId = createdCart.UserId }, _response);
            }
            catch (InvalidOperationException ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return Conflict(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while creating the cart." + ex;
                return StatusCode(500, _response);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCart([FromBody] Cart cart)
        {
            try
            {
                var updatedCart = await _cartService.UpdateCart(cart);
                _response.Result = updatedCart;
                _response.Message = "Cart updated successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while updating the cart." + ex;
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveCart(string userId)
        {
            try
            {
                var result = await _cartService.RemoveCart(userId);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Cart not found or could not be removed.";
                    return NotFound(_response);
                }

                _response.Message = "Cart removed successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while removing the cart." + ex;
                return StatusCode(500, _response);
            }
        }
    }
}
