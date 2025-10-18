using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using WebApi.Common.Exceptions;
using WebApi.Configuration;
using WebApi.Data;
using WebApi.Dto;
using WebApi.Interface;

namespace WebApi.Service
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly StripeSettings _stripeSettings;

        public CheckoutService(
            ApplicationDbContext context,
            IProductService productService,
            IOptions<StripeSettings> stripeSettings)
        {
            _context = context;
            _productService = productService;
            _stripeSettings = stripeSettings.Value;

            // Set Stripe API key
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        public async Task<StripeResponseDto> CreateSessionAsync(List<CheckoutItemRequestDto> checkoutItems)
        {
            if (checkoutItems == null || checkoutItems.Count == 0)
            {
                throw new ArgumentException("Checkout items cannot be empty");
            }

            // Build line items for Stripe session
            var lineItems = new List<SessionLineItemOptions>();
            decimal totalAmount = 0;

            foreach (var item in checkoutItems)
            {
                var product = await _productService.GetProductById(item.ProductId);

                if (product.Quantity < item.Quantity)
                {
                    throw new ProductNotPresentException(
                        $"Insufficient quantity for product {product.Name}. Available: {product.Quantity}, Requested: {item.Quantity}");
                }

                var lineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = (long)(product.Price * 100), // Stripe uses cents
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = product.Name,
                            Description = product.Category?.Name ?? "Product",
                            Images = new List<string> { } // Add image URLs if available
                        }
                    },
                    Quantity = item.Quantity
                };

                lineItems.Add(lineItem);
                totalAmount += (product.Price * item.Quantity);
            }

            // Create Stripe checkout session
            var sessionOptions = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = _stripeSettings.SuccessUrl,
                CancelUrl = _stripeSettings.CancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "OrderDate", DateTime.UtcNow.ToString("O") },
                    { "TotalAmount", totalAmount.ToString() }
                }
            };

            try
            {
                var service = new SessionService();
                var session = await service.CreateAsync(sessionOptions);

                var stripeResponse = new StripeResponseDto
                {
                    SessionId = session.Id,
                    PublicKey = _stripeSettings.PublishableKey,
                    ClientSecret = session.ClientSecret,
                    PaymentUrl = session.Url
                };

                return stripeResponse;
            }
            catch (StripeException ex)
            {
                throw new Exception($"Stripe error: {ex.Message}", ex);
            }
        }
    }
}
