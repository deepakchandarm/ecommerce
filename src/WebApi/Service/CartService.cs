using Microsoft.EntityFrameworkCore;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Data;
using WebApi.Interface;

namespace WebApi.Service
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartAsync(long cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                throw new ResourceNotFoundException($"Cart with ID {cartId} not found");
            }

            return cart;
        }

        public async Task ClearCartAsync(long cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                throw new ResourceNotFoundException($"Cart with ID {cartId} not found");
            }

            cart.Items.Clear();
            cart.TotalAmount = 0;

            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalPriceAsync(long cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                throw new ResourceNotFoundException($"Cart with ID {cartId} not found");
            }

            return cart.Items.Sum(item => item.Amount);
        }

        public Cart InitializeNewCart(User user)
        {
            var existingCart = _context.Carts
                .FirstOrDefault(c => c.UserId == user.Id);

            if (existingCart != null)
            {
                return existingCart;
            }

            var newCart = new Cart
            {
                UserId = user.Id,
                TotalAmount = 0,
                Items = new List<CartItem>()
            };

            _context.Carts.Add(newCart);
            _context.SaveChanges();
            return newCart;
        }

        public Cart GetCartById(int cartId)
        {
            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.Id == cartId);

            if (cart == null)
            {
                throw new ResourceNotFoundException($"Cart with id {cartId} not found");
            }

            return cart;
        }
    }
}
