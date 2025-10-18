using Microsoft.EntityFrameworkCore;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Data;
using WebApi.Interface;

namespace WebApi.Service
{
    public class CartItemService : ICartItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public CartItemService(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task AddCartItem(int cartId, int productId, int quantity)
        {
            var product = await _productService.GetProductById(productId);

            if (product.Quantity < quantity)
            {
                throw new ProductNotPresentException("Insufficient product quantity available");
            }

            var existingItem = _context.CartItems
                .FirstOrDefault(ci => ci.CartId == cartId && ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.Amount = existingItem.Quantity * product.Price;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = quantity,
                    Amount = quantity * product.Price
                };
                await _context.CartItems.AddAsync(newItem);
            }

            await UpdateCartTotal(cartId);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItem(int cartId, int itemId)
        {
            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem == null || cartItem.CartId != cartId)
            {
                throw new ResourceNotFoundException($"Cart item with id {itemId} not found in cart {cartId}");
            }

            _context.CartItems.Remove(cartItem);
            await UpdateCartTotal(cartId);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemQuantity(int cartId, int itemId, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0");
            }

            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem == null || cartItem.CartId != cartId)
            {
                throw new ResourceNotFoundException($"Cart item with id {itemId} not found in cart {cartId}");
            }

            var product = await _productService.GetProductById(cartItem.ProductId);
            if (product.Quantity < quantity)
            {
                throw new ProductNotPresentException("Insufficient product quantity available");
            }

            cartItem.Quantity = quantity;
            cartItem.Amount = quantity * product.Price;

            await UpdateCartTotal(cartId);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateCartTotal(int cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart != null)
            {
                cart.TotalAmount = cart.Items.Sum(item => item.Amount);
            }
        }
    }
}
