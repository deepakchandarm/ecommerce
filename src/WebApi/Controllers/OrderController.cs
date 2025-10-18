using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Dto;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Dto;
using WebApi.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new order from the user's cart.
        /// </summary>
        /// <param name="userId">The ID of the user placing the order</param>
        /// <returns>ResponseEntity containing ApiResponse with order details</returns>
        [HttpPost("order")]
        public async Task<IActionResult> CreateOrder([FromQuery] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new ApiResponse<Order>("Invalid user ID", null));
                }

                var order = await _orderService.PlaceOrderAsync(userId);
                var orderDto = _orderService.ConvertToDto(order);

                _logger.LogInformation($"Order created successfully for user {userId}");
                return Ok(new ApiResponse<OrderDto>("Order created successfully!", orderDto));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"Resource not found: {ex.Message}");
                return NotFound(new ApiResponse<string>("Oops!", ex.Message));
            }
            catch (ProductNotPresentException ex)
            {
                _logger.LogWarning($"Product not available: {ex.Message}");
                return BadRequest(new ApiResponse<string>("Error Occurred", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return BadRequest(new ApiResponse<string>("Error Occurred", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while creating order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("Error Occurred", ex.Message));
            }
        }

        /// <summary>
        /// Retrieve a specific order by its ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve</param>
        /// <returns>ResponseEntity containing ApiResponse with order details</returns>
        [HttpGet("{orderId}/order")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    return BadRequest(new ApiResponse<Order>("Invalid order ID", null));
                }

                var order = await _orderService.GetOrderAsync(orderId);
                return Ok(new ApiResponse<OrderDto>("Order fetched successfully!", order));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"Order not found: {ex.Message}");
                return NotFound(new ApiResponse<string>("Oops!", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("Error Occurred", ex.Message));
            }
        }

        /// <summary>
        /// Retrieve all orders for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose orders to retrieve</param>
        /// <returns>ResponseEntity containing ApiResponse with list of orders</returns>
        [HttpGet("{userId}/orders")]
        public async Task<IActionResult> GetOrdersByUserId(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new ApiResponse<Order>("Invalid user ID", null));
                }

                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(new ApiResponse<List<OrderDto>>("Order fetched successfully!", orders));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"Resource not found: {ex.Message}");
                return NotFound(new ApiResponse<string>("Oops!", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching orders: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("Error Occurred", ex.Message));
            }
        }
    }
}
