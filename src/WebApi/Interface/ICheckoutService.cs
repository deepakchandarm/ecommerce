using WebApi.Dto;

namespace WebApi.Interface
{
    public interface ICheckoutService
    {
        Task<StripeResponseDto> CreateSessionAsync(List<CheckoutItemRequestDto> checkoutItems);
    }
}
