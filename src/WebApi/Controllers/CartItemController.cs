using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Dto;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/cartItems")]
    [Authorize]
    public class CartItemController : ControllerBase
    {
        private readonly ICartItemService _cartItemService;
        private readonly ICartService _cartService;
        private readonly IUserService _userService;

        public CartItemController(
            ICartItemService cartItemService,
            ICartService cartService,
            IUserService userService)
        {
            _cartItemService = cartItemService;
            _cartService = cartService;
            _userService = userService;
        }

        [HttpPost("item/add")]
        public async Task<IActionResult> AddItemToCart([FromQuery] int productId, [FromQuery] int quantity)
        {
            try
            {
                var user = await _userService.GetAuthenticatedUser();
                var cart = _cartService.InitializeNewCart(user);
                await _cartItemService.AddCartItem(cart.Id, productId, quantity);
                return Ok(new ApiResponse<CartItem>("Item added to cart", null));
            }
            catch (ProductNotPresentException ex)
            {
                return NotFound(new ApiResponse<CartItem>(ex.Message, null));
            }
            catch (JwtAuthenticationException ex)
            {
                return Unauthorized(new ApiResponse<CartItem>(ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartItem>("Internal server error", null));
            }
        }

        [HttpDelete("cart/{cartId}/item/{itemId}/remove")]
        public async Task<IActionResult> RemoveItem(int cartId, int itemId)
        {
            try
            {
                await _cartItemService.RemoveCartItem(cartId, itemId);
                return Ok(new ApiResponse<CartItem>("Item removed from cart", null));
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new ApiResponse<CartItem>(ex.Message, null));
            }
            catch (ProductNotPresentException ex)
            {
                return NotFound(new ApiResponse<CartItem>(ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartItem>("Internal server error", null));
            }
        }

        [HttpPut("cart/{cartId}/item/{itemId}/update")]
        public async Task<IActionResult> UpdateItemQuantity(int cartId, int itemId, [FromQuery] int quantity)
        {
            try
            {
                await _cartItemService.UpdateItemQuantity(cartId, itemId, quantity);
                return Ok(new ApiResponse<CartItem>("Item updated", null));
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new ApiResponse<CartItem>(ex.Message, null));
            }
            catch (ProductNotPresentException ex)
            {
                return NotFound(new ApiResponse<CartItem>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<CartItem>(ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartItem>("Internal server error", null));
            }
        }
    }
}
