using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using WebApi.Configuration;
using WebApi.Dao;
using WebApi.Data;
using WebApi.Interface;

namespace WebApi.Service
{
    public class StripeReconciliationService : IStripeReconciliationService
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<StripeReconciliationService> _logger;

        public StripeReconciliationService(
            ApplicationDbContext context,
            IOptions<StripeSettings> stripeSettings,
            ILogger<StripeReconciliationService> logger)
        {
            _context = context;
            _stripeSettings = stripeSettings.Value;
            _logger = logger;

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        /// <summary>
        /// Reconciles all pending orders with their Stripe payment intents.
        /// Updates order status based on payment intent status.
        /// </summary>
        public async Task ReconcilePaymentsAsync()
        {
            try
            {
                _logger.LogInformation("Starting payment reconciliation process...");

                // Get all orders with pending or processing status
                var pendingOrders = await _context.Orders
                    .Where(o => o.OrderStatus == "Pending" || o.PaymentStatus == "processing")
                    .Include(o => o.User)
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .ToListAsync();

                _logger.LogInformation($"Found {pendingOrders.Count} pending orders to reconcile");

                if (pendingOrders.Count == 0)
                {
                    _logger.LogInformation("No pending orders to reconcile");
                    return;
                }

                var paymentIntentService = new PaymentIntentService();
                int successCount = 0;
                int failedCount = 0;

                foreach (var order in pendingOrders)
                {
                    try
                    {
                        // Skip if no payment intent ID
                        if (string.IsNullOrWhiteSpace(order.PaymentIntentId))
                        {
                            _logger.LogWarning($"Order {order.Id} has no PaymentIntentId");
                            continue;
                        }

                        // Retrieve payment intent from Stripe
                        var paymentIntent = await paymentIntentService.GetAsync(order.PaymentIntentId);

                        if (paymentIntent == null)
                        {
                            _logger.LogWarning($"Payment intent {order.PaymentIntentId} not found in Stripe");
                            continue;
                        }

                        _logger.LogInformation($"Order {order.Id}: Payment Intent Status = {paymentIntent.Status}");

                        // Process based on payment status
                        switch (paymentIntent.Status)
                        {
                            case "succeeded":
                                await ProcessSuccessfulPaymentAsync(order, paymentIntent);
                                successCount++;
                                break;

                            case "processing":
                                await ProcessProcessingPaymentAsync(order, paymentIntent);
                                break;

                            case "requires_payment_method":
                            case "requires_action":
                                await ProcessFailedPaymentAsync(order, paymentIntent);
                                failedCount++;
                                break;

                            case "canceled":
                                await ProcessCancelledPaymentAsync(order, paymentIntent);
                                failedCount++;
                                break;

                            default:
                                _logger.LogWarning($"Unknown payment status '{paymentIntent.Status}' for order {order.Id}");
                                break;
                        }
                    }
                    catch (StripeException ex)
                    {
                        _logger.LogError($"Stripe error processing order {order.Id}: {ex.Message}");
                        failedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Unexpected error processing order {order.Id}: {ex.Message}");
                        failedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"Payment reconciliation completed. Success: {successCount}, Failed: {failedCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fatal error during payment reconciliation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verifies a single payment intent status.
        /// </summary>
        public async Task<bool> VerifyPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentIntentId))
                {
                    throw new ArgumentException("Payment Intent ID cannot be empty");
                }

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId);

                _logger.LogInformation($"Verified payment intent {paymentIntentId}: Status = {paymentIntent.Status}");

                return paymentIntent.Status == "succeeded";
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error verifying payment intent {paymentIntentId}: {ex.Message}");
                throw new InvalidOperationException($"Failed to verify payment intent: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Processes webhook events from Stripe.
        /// Called when Stripe sends payment status updates.
        /// </summary>
        public async Task ProcessPaymentWebhookAsync(string paymentIntentId)
        {
            try
            {
                _logger.LogInformation($"Processing webhook for payment intent {paymentIntentId}");

                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.PaymentIntentId == paymentIntentId);

                if (order == null)
                {
                    _logger.LogWarning($"Order not found for payment intent {paymentIntentId}");
                    return;
                }

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId);

                switch (paymentIntent.Status)
                {
                    case "succeeded":
                        await ProcessSuccessfulPaymentAsync(order, paymentIntent);
                        break;

                    case "processing":
                        await ProcessProcessingPaymentAsync(order, paymentIntent);
                        break;

                    case "requires_payment_method":
                    case "requires_action":
                        await ProcessFailedPaymentAsync(order, paymentIntent);
                        break;

                    case "canceled":
                        await ProcessCancelledPaymentAsync(order, paymentIntent);
                        break;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Webhook processed for order {order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing webhook for payment intent {paymentIntentId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retries failed payments.
        /// Used for manual retry logic.
        /// </summary>
        public async Task RetryFailedPaymentsAsync()
        {
            try
            {
                _logger.LogInformation("Starting failed payment retry process...");

                var failedOrders = await _context.Orders
                    .Where(o => o.OrderStatus == "Failed" && o.PaymentStatus == "failed")
                    .Include(o => o.User)
                    .ToListAsync();

                _logger.LogInformation($"Found {failedOrders.Count} failed orders to retry");

                // Note: You might want to add retry count logic to prevent infinite retries
                foreach (var order in failedOrders)
                {
                    // Create new payment intent for retry
                    var paymentIntentOptions = new PaymentIntentCreateOptions
                    {
                        Amount = (long)(order.TotalAmount * 100), // Convert to cents
                        Currency = "usd",
                        PaymentMethod = null, // User will provide new payment method
                        Metadata = new Dictionary<string, string>
                        {
                            { "orderId", order.Id.ToString() },
                            { "userId", order.UserId.ToString() },
                            { "retry", "true" }
                        }
                    };

                    var paymentIntentService = new PaymentIntentService();
                    var newPaymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

                    // Update order with new payment intent ID
                    order.PaymentIntentId = newPaymentIntent.Id;
                    order.PaymentStatus = "processing";
                    order.UpdatedDate = DateTime.UtcNow;

                    _logger.LogInformation($"Created new payment intent {newPaymentIntent.Id} for failed order {order.Id}");
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Failed payment retry process completed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrying failed payments: {ex.Message}");
                throw;
            }
        }

        // ==================== Private Helper Methods ====================

        private async Task ProcessSuccessfulPaymentAsync(Order order, PaymentIntent paymentIntent)
        {
            _logger.LogInformation($"Processing successful payment for order {order.Id}");

            // Update order status
            order.OrderStatus = "Confirmed";
            order.PaymentStatus = "succeeded";
            order.PaidDate = DateTime.UtcNow;
            order.UpdatedDate = DateTime.UtcNow;

            _context.Orders.Update(order);

            _logger.LogInformation($"Order {order.Id} status updated to Confirmed");
        }

        private async Task ProcessProcessingPaymentAsync(Order order, PaymentIntent paymentIntent)
        {
            _logger.LogInformation($"Payment for order {order.Id} is still processing");

            // Update status but keep as pending
            order.PaymentStatus = "processing";
            order.UpdatedDate = DateTime.UtcNow;

            _context.Orders.Update(order);

            _logger.LogInformation($"Order {order.Id} status remains Pending (processing)");
        }

        private async Task ProcessFailedPaymentAsync(Order order, PaymentIntent paymentIntent)
        {
            _logger.LogInformation($"Processing failed payment for order {order.Id}");

            // Update order status
            order.OrderStatus = "Failed";
            order.PaymentStatus = "failed";
            order.UpdatedDate = DateTime.UtcNow;

            _context.Orders.Update(order);

            // Restore product quantities to inventory
            await RestoreInventoryAsync(order);

            _logger.LogInformation($"Order {order.Id} status updated to Failed, inventory restored");
        }

        private async Task ProcessCancelledPaymentAsync(Order order, PaymentIntent paymentIntent)
        {
            _logger.LogInformation($"Processing cancelled payment for order {order.Id}");

            // Update order status
            order.OrderStatus = "Cancelled";
            order.PaymentStatus = "cancelled";
            order.UpdatedDate = DateTime.UtcNow;

            _context.Orders.Update(order);

            // Restore product quantities to inventory
            await RestoreInventoryAsync(order);

            _logger.LogInformation($"Order {order.Id} status updated to Cancelled, inventory restored");
        }

        private async Task RestoreInventoryAsync(Order order)
        {
            try
            {
                _logger.LogInformation($"Restoring inventory for order {order.Id}");

                foreach (var item in order.Items)
                {
                    var product = item.Product;
                    if (product != null)
                    {
                        product.Quantity += item.Quantity;
                        _context.Products.Update(product);

                        _logger.LogInformation(
                            $"Restored {item.Quantity} units of product {product.Name} (ID: {product.Id})");
                    }
                }

                _logger.LogInformation($"Inventory restoration completed for order {order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error restoring inventory for order {order.Id}: {ex.Message}");
                throw;
            }
        }
    }
}
