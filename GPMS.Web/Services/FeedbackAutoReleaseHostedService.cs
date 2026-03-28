using GPMS.Application.Interfaces.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GPMS.Web.Services;

public class FeedbackAutoReleaseHostedService : BackgroundService
{
    private readonly ILogger<FeedbackAutoReleaseHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public FeedbackAutoReleaseHostedService(
        ILogger<FeedbackAutoReleaseHostedService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[Service] FeedbackAutoReleaseHostedService is Starting.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var autoReleaseService = scope.ServiceProvider.GetRequiredService<IFeedbackAutoReleaseService>();
                    var releasedCount = await autoReleaseService.AutoReleasePendingFeedbacksAsync(stoppingToken);

                    if (releasedCount > 0)
                    {
                        Console.WriteLine($"[Service] Auto-released {releasedCount} feedback approval(s).");
                        _logger.LogInformation("Auto-released {ReleasedCount} feedback approval(s).", releasedCount);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Service] FeedbackAutoReleaseHostedService Internal ERROR: {ex.Message}");
                    _logger.LogError(ex, "Error occurred executing FeedbackAutoReleaseHostedService.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Service] FeedbackAutoReleaseHostedService CRITICAL: {ex.Message}");
        }

        Console.WriteLine("[Service] FeedbackAutoReleaseHostedService is Stopped.");
    }
}
