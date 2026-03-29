using GPMS.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class ReminderHostedService : BackgroundService
{
    private readonly ILogger<ReminderHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ReminderHostedService(
        ILogger<ReminderHostedService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderHostedService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                await reminderService.ProcessAllRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing ReminderHostedService.");
            }

            // Check every 6 hours
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }

        _logger.LogInformation("ReminderHostedService is stopping.");
    }
}
