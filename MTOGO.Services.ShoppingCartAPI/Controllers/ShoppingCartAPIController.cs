using Microsoft.AspNetCore.Mvc;
using MTOGO.MessageBus;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Services.IServices;
using Newtonsoft.Json;

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
            var cart = await _cartService.GetCartAsync(userId);
            return cart == null ? NotFound() : Ok(cart);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart([FromBody] Cart cart)
        {
            var updatedCart = await _cartService.UpdateCartAsync(cart);
            await _messageBus.PublishMessage("CartUpdatedQueue", JsonConvert.SerializeObject(cart));
            return Ok(updatedCart);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveCart(string userId)
        {
            await _cartService.RemoveCartAsync(userId);
            await _messageBus.PublishMessage("CartRemovedQueue", $"Cart for user {userId} removed");
            return NoContent();
        }
    }
}
