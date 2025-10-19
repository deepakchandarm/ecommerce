namespace WebApi.Interface
{
    public interface IStripeReconciliationService
    {
        Task ReconcilePaymentsAsync();
        Task<bool> VerifyPaymentIntentAsync(string paymentIntentId);
        Task ProcessPaymentWebhookAsync(string paymentIntentId);
        Task RetryFailedPaymentsAsync();
    }
}
