using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Dto;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/carts")]
    [Produces("application/json")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Get cart by ID
        /// </summary>
        /// <param name="cartId">The cart ID</param>
        /// <returns>Cart details</returns>
        /// <response code="200">Returns the cart</response>
        /// <response code="404">Cart not found</response>
        [HttpGet("{cartId}/my-cart")]
        [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Cart>>> GetCart([FromRoute] long cartId)
        {
            try
            {
                var cart = await _cartService.GetCartAsync(cartId);
                return Ok(new ApiResponse<Cart>("Success", cart));
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(ex.Message, null));
            }
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        /// <param name="cartId">The cart ID</param>
        /// <returns>Success message</returns>
        /// <response code="200">Cart cleared successfully</response>
        /// <response code="404">Cart not found</response>
        [HttpDelete("{cartId}/clear-cart")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ClearCart([FromRoute] long cartId)
        {
            try
            {
                await _cartService.ClearCartAsync(cartId);
                return Ok(new ApiResponse<object>("Clear Cart Success!", null));
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(ex.Message, null));
            }
        }

        /// <summary>
        /// Get total price of cart
        /// </summary>
        /// <param name="cartId">The cart ID</param>
        /// <returns>Total cart price</returns>
        /// <response code="200">Returns the total price</response>
        /// <response code="404">Cart not found</response>
        [HttpGet("{cartId}/cart/total-price")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalAmount([FromRoute] long cartId)
        {
            try
            {
                var totalAmount = await _cartService.GetTotalPriceAsync(cartId);
                return Ok(new ApiResponse<decimal>("Total Price", totalAmount));
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(ex.Message, null));
            }
        }
    }
}
