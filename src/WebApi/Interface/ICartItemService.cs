namespace WebApi.Interface
{
    public interface ICartItemService
    {
        Task AddCartItem(int cartId, int productId, int quantity);
        Task RemoveCartItem(int cartId, int itemId);
        Task UpdateItemQuantity(int cartId, int itemId, int quantity);
    }
}
