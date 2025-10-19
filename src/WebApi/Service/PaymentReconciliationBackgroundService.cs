using WebApi.Interface;

public class PaymentReconciliationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentReconciliationBackgroundService> _logger;
    private readonly TimeSpan _reconciliationInterval = TimeSpan.FromMinutes(5); // Run every 5 minutes

    public PaymentReconciliationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PaymentReconciliationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Reconciliation Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var reconciliationService = scope.ServiceProvider
                        .GetRequiredService<IStripeReconciliationService>();

                    _logger.LogInformation("Running automatic payment reconciliation...");
                    await reconciliationService.ReconcilePaymentsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in background reconciliation service: {ex.Message}");
            }

            await Task.Delay(_reconciliationInterval, stoppingToken);
        }

        _logger.LogInformation("Payment Reconciliation Background Service stopped");
    }
}