using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using GPMS.Domain.Enums;

namespace GPMS.Web.Services
{
    public class FeedbackAutoReleaseService : BackgroundService
    {
        private readonly ILogger<FeedbackAutoReleaseService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public FeedbackAutoReleaseService(ILogger<FeedbackAutoReleaseService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FeedbackAutoReleaseService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAutoReleaseAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing FeedbackAutoReleaseService.");
                }

                // Run once every 24 hours (or adjust for testing)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("FeedbackAutoReleaseService is stopping.");
        }

        private async Task ProcessAutoReleaseAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<GpmsDbContext>();

                // Time threshold: Release feedback after 7 days if approved and not yet released
                var thresholdDate = DateTime.UtcNow.AddDays(-7);

                var pendingReleases = await context.FeedbackApprovals
                    .Where(fa => fa.ApprovalStatus == ApprovalStatus.Approved && 
                                 !fa.IsVisibleToStudent && 
                                 fa.ApprovedAt <= thresholdDate)
                    .ToListAsync(stoppingToken);

                if (pendingReleases.Any())
                {
                    _logger.LogInformation($"Found {pendingReleases.Count} feedback(s) to auto-release.");

                    foreach (var approval in pendingReleases)
                    {
                        approval.IsVisibleToStudent = true;
                        approval.AutoReleasedAt = DateTime.UtcNow;
                    }

                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation($"Successfully auto-released {pendingReleases.Count} feedback(s).");
                }
            }
        }
    }
}
