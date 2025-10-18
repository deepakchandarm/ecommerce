using WebApi.Dao;
using WebApi.Dto;

namespace WebApi.Interface
{
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(int userId);
        Task<OrderDto> GetOrderAsync(int orderId);
        Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId);
        OrderDto ConvertToDto(Order order);
    }
}
