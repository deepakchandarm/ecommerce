using Microsoft.EntityFrameworkCore;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Data;
using WebApi.Dto;
using WebApi.Interface;

namespace WebApi.Service
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ApplicationDbContext context,
            ICartService cartService,
            IProductService productService,
            ILogger<OrderService> logger)
        {
            _context = context;
            _cartService = cartService;
            _productService = productService;
            _logger = logger;
        }

        public async Task<Order> PlaceOrderAsync(int userId)
        {
            try
            {
                // Verify user exists
                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    throw new ResourceNotFoundException($"User with id {userId} not found");
                }

                // Get user's cart
                var cart = _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.UserId == userId);

                if (cart == null || cart.Items.Count == 0)
                {
                    throw new InvalidOperationException($"Cart is empty for user {userId}");
                }

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderStatus = "Pending",
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                // Create order items from cart items
                foreach (var cartItem in cart.Items)
                {
                    var product = cartItem.Product;

                    // Verify product quantity is still available
                    if (product.Quantity < cartItem.Quantity)
                    {
                        throw new ProductNotPresentException(
                            $"Insufficient quantity for product {product.Name}");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        Price = product.Price,
                        Amount = cartItem.Amount
                    };

                    order.Items.Add(orderItem);
                    totalAmount += orderItem.Amount;

                    // Update product quantity
                    product.Quantity -= cartItem.Quantity;
                    _context.Products.Update(product);
                }

                order.TotalAmount = totalAmount;

                // Save order
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order {order.Id} created successfully for user {userId}");

                // Clear the cart after order is created
                _context.CartItems.RemoveRange(cart.Items);
                cart.TotalAmount = 0;
                await _context.SaveChangesAsync();

                return order;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while creating order: {ex.Message}");
                throw new InvalidOperationException("Error creating order", ex);
            }
        }

        public async Task<OrderDto> GetOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new ResourceNotFoundException($"Order with id {orderId} not found");
            }

            return ConvertToDto(order);
        }

        public async Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            // Verify user exists
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                throw new ResourceNotFoundException($"User with id {userId} not found");
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            if (orders.Count == 0)
            {
                _logger.LogWarning($"No orders found for user {userId}");
            }

            return orders.Select(ConvertToDto).ToList();
        }

        public OrderDto ConvertToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.OrderStatus,
                CreatedDate = order.CreatedDate,
                UpdatedDate = order.UpdatedDate,
                Items = order.Items?.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Unknown Product",
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Amount = oi.Amount
                }).ToList() ?? new List<OrderItemDto>()
            };
        }
    }
}
