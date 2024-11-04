using Microsoft.AspNetCore.Mvc;
using MTOGO.MessageBus;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Services.IServices;

namespace MTOGO.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/shoppingcart")]
    public class ShoppingCartAPIController : ControllerBase
    {
        private readonly IShoppingCartService _cartService;
        private readonly IMessageBus _messageBus;

        public ShoppingCartAPIController(IShoppingCartService cartService, IMessageBus messageBus)
        {
            _cartService = cartService;
            _messageBus = messageBus;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            var cart = await _cartService.GetCart(userId);
            return cart == null ? NotFound() : Ok(cart);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCart([FromBody] Cart cart)
        {
            try
            {
                var createdCart = await _cartService.CreateCart(cart);
                return CreatedAtAction(nameof(GetCart), new { userId = createdCart.UserId }, createdCart);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCart([FromBody] Cart cart)
        {
            var updatedCart = await _cartService.UpdateCart(cart);
            return Ok(updatedCart);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveCart(string userId)
        {
            await _cartService.RemoveCart(userId);
            return NoContent();
        }

    }
}
