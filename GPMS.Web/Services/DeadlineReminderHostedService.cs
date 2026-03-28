using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GPMS.Web.Services;

public class DeadlineReminderHostedService : BackgroundService
{
    private readonly ILogger<DeadlineReminderHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DeadlineReminderHostedService(
        ILogger<DeadlineReminderHostedService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[Service] DeadlineReminderHostedService is Starting.");

        try 
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    var errMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    Console.WriteLine($"[Service] DeadlineReminderHostedService Internal ERROR: {errMsg}");
                    _logger.LogError(ex, "Error occurred executing DeadlineReminderHostedService.");
                }

                // Wait 6 hours, catching cancellation
                try 
                {
                    await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal stop
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Service] DeadlineReminderHostedService CRITICAL CRASH: {ex.Message}");
        }

        Console.WriteLine("[Service] DeadlineReminderHostedService is Stopped.");
    }

    private async Task ProcessRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GpmsDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var today = DateTime.UtcNow.Date;

        // 1. Get all active submission requirements from ongoing rounds
        var requirements = await context.SubmissionRequirements
            .Include(r => r.ReviewRound)
            .Where(r => r.ReviewRound.Status == RoundStatus.Ongoing)
            .ToListAsync(stoppingToken);

        int notificationsCreated = 0;

        foreach (var req in requirements)
        {
            var daysUntil = (req.Deadline.Date - today).Days;
            var members = await GetGroupMembersForRequirement(context, req);

            if (members.Any())
            {
                // Handle Overdue Reminders
                if (daysUntil < 0)
                {
                    notificationsCreated += await SendNotificationsToMembersIfMissingAsync(context, emailService, req, members, type: "Overdue", stoppingToken);
                }
                // Handle Urgent Reminders (1 day or less)
                else if (daysUntil <= 1)
                {
                    notificationsCreated += await SendNotificationsToMembersIfMissingAsync(context, emailService, req, members, type: "Urgent", stoppingToken);
                }
                // Handle Upcoming Reminders (between 1 and 3 days)
                else if (daysUntil <= 3)
                {
                    notificationsCreated += await SendNotificationsToMembersIfMissingAsync(context, emailService, req, members, type: "Upcoming", stoppingToken);
                }
            }
        }

        if (notificationsCreated > 0)
        {
            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Successfully created {Count} deadline notifications (including overdue) and triggered emails.", notificationsCreated);
        }
    }

    private async Task<List<User>> GetGroupMembersForRequirement(GpmsDbContext context, SubmissionRequirement req)
    {
        return await context.ProjectGroups
            .Include(g => g.Project)
            .Include(g => g.GroupMembers)
                .ThenInclude(m => m.User)
            .Where(g => g.Project.SemesterID == req.ReviewRound.SemesterID)
            // Filter groups that haven't submitted this requirement
            .Where(g => !context.Submissions.Any(s => s.GroupID == g.GroupID && s.RequirementID == req.RequirementID))
            .SelectMany(g => g.GroupMembers.Select(m => m.User))
            .ToListAsync();
    }

    private async Task<int> SendNotificationsToMembersIfMissingAsync(GpmsDbContext context, IEmailService emailService, SubmissionRequirement req, List<User> members, string type, CancellationToken stoppingToken)
    {
        int count = 0;
        string title;
        string content;
        
        var today = DateTime.UtcNow.Date;
        var daysUntil = (req.Deadline.Date - today).Days;

        if (type == "Overdue")
        {
            title = "Overdue: Submission Missing";
            content = $"Your submission for '{req.DocumentName}' is OVERDUE by {Math.Abs(daysUntil)} day(s). Please submit it immediately.";
        }
        else if (type == "Urgent")
        {
            title = "Urgent: Deadline Tomorrow";
            content = $"Your submission for '{req.DocumentName}' is due in {Math.Max(0, daysUntil)} day(s). Please submit it on time.";
        }
        else
        {
            title = "Upcoming Deadline";
            content = $"Your submission for '{req.DocumentName}' is due in {daysUntil} day(s). Please submit it on time.";
        }

        foreach (var member in members)
        {
            // Logic for avoiding duplicate notifications:
            // - If "Overdue": Check if they already have an "Overdue" notification for this requirement.
            // - If "Urgent": Check if they already have an "Urgent" OR "Overdue" notification.
            // - If "Upcoming": Check if they already have ANY notification for this requirement.
            bool alreadyNotified = await context.Notifications
                .AnyAsync(n => n.RecipientID == member.UserID 
                            && n.Type == NotificationType.Deadline 
                            && n.RelatedEntityID == req.RequirementID
                            && (type == "Overdue" ? n.Title.Contains("Overdue") : 
                                type == "Urgent" ? (n.Title.Contains("Urgent") || n.Title.Contains("Overdue")) : true), stoppingToken);

            if (!alreadyNotified)
            {
                context.Notifications.Add(new Notification
                {
                    RecipientID = member.UserID,
                    Title = title,
                    Content = content,
                    Type = NotificationType.Deadline,
                    RelatedEntityType = "SubmissionRequirement",
                    RelatedEntityID = req.RequirementID,
                    CreatedAt = DateTime.UtcNow
                });

                // Send email
                if (!string.IsNullOrEmpty(member.Email))
                {
                    try
                    {
                        var subject = $"[GPMS] {title}: {req.DocumentName}";
                        var body = $@"
                            <h2>{title}</h2>
                            <p>Hello {member.FullName},</p>
                            <p>{content}</p>
                            <p><strong>Original Deadline:</strong> {req.Deadline:yyyy-MM-dd HH:mm}</p>
                            <br/>
                            <p>This is an automated message from the Graduation Project Management System.</p>";

                        await emailService.SendEmailAsync(member.Email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send deadline reminder email to {Email}", member.Email);
                    }
                }

                count++;
            }
        }
        return count;
    }
}
