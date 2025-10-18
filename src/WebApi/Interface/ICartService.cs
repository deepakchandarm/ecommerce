using WebApi.Dao;

namespace WebApi.Interface
{
    public interface ICartService
    {
        Task<Cart> GetCartAsync(long cartId);
        Task ClearCartAsync(long cartId);
        Task<decimal> GetTotalPriceAsync(long cartId);
        Cart InitializeNewCart(User user);
        Cart GetCartById(int cartId);
    }
}
