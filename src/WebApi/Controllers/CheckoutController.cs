using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Dto;
using WebApi.Common.Exceptions;
using WebApi.Dto;
using WebApi.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/checkout")]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICheckoutService checkoutService,
            ILogger<CheckoutController> logger)
        {
            _checkoutService = checkoutService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint to create a payment session for the provided checkout items.
        /// </summary>
        /// <param name="checkoutItemDtoList">List of items for the checkout session</param>
        /// <returns>ResponseEntity containing ApiResponse with Stripe session details or an error message</returns>
        [HttpPost("create-session")]
        public async Task<IActionResult> CreateCheckoutSession(
            [FromBody] List<CheckoutItemRequestDto> checkoutItemDtoList)
        {
            try
            {
                if (checkoutItemDtoList == null || checkoutItemDtoList.Count == 0)
                {
                    return BadRequest(new ApiResponse<CheckoutItemRequestDto>("Checkout items list cannot be empty", null));
                }

                // Create a Stripe checkout session
                var stripeResponse = await _checkoutService.CreateSessionAsync(checkoutItemDtoList);

                return Ok(new ApiResponse<StripeResponseDto>("Checkout session created successfully!", stripeResponse));
            }
            catch (ProductNotPresentException ex)
            {
                _logger.LogWarning($"Product not found during checkout: {ex.Message}");
                return NotFound(new ApiResponse<CheckoutItemRequestDto>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid checkout request: {ex.Message}");
                return BadRequest(new ApiResponse<CheckoutItemRequestDto>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Stripe error: {ex.Message}");

                if (ex.Message.Contains("Stripe error"))
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new ApiResponse<CheckoutItemRequestDto>($"Stripe error: {ex.InnerException?.Message ?? ex.Message}", null));
                }

                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CheckoutItemRequestDto>($"An unexpected error occurred: {ex.Message}", null));
            }
        }
    }
}
