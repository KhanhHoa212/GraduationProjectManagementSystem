using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GPMS.Web.Services;

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
                await ProcessAllRemindersAsync(stoppingToken);
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

    private async Task ProcessAllRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GpmsDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        int deadlineNotifications = await ProcessDeadlineRemindersAsync(context, emailService, stoppingToken);
        int sessionNotifications = await ProcessReviewSessionRemindersAsync(context, emailService, stoppingToken);

        if (deadlineNotifications > 0 || sessionNotifications > 0)
        {
            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Successfully processed reminders. Created {DeadlineCount} deadline and {SessionCount} session notifications.", deadlineNotifications, sessionNotifications);
        }
    }

    #region Deadline Reminders

    private async Task<int> ProcessDeadlineRemindersAsync(GpmsDbContext context, IEmailService emailService, CancellationToken stoppingToken)
    {
        var today = DateTime.UtcNow.Date;
        int count = 0;

        // Get active submission requirements from ongoing rounds
        var requirements = await context.SubmissionRequirements
            .Include(r => r.ReviewRound)
            .Where(r => r.ReviewRound.Status == RoundStatus.Ongoing)
            .ToListAsync(stoppingToken);

        foreach (var req in requirements)
        {
            var daysUntil = (req.Deadline.Date - today).Days;
            var members = await GetGroupMembersForRequirement(context, req);

            if (members.Any())
            {
                if (daysUntil < 0)
                {
                    count += await SendDeadlineNotificationsIfMissingAsync(context, emailService, req, members, "Overdue", stoppingToken);
                }
                else if (daysUntil <= 1)
                {
                    count += await SendDeadlineNotificationsIfMissingAsync(context, emailService, req, members, "Urgent", stoppingToken);
                }
                else if (daysUntil <= 3)
                {
                    count += await SendDeadlineNotificationsIfMissingAsync(context, emailService, req, members, "Upcoming", stoppingToken);
                }
            }
        }
        return count;
    }

    private async Task<List<User>> GetGroupMembersForRequirement(GpmsDbContext context, SubmissionRequirement req)
    {
        var semesterId = req.ReviewRound.SemesterID;
        var requirementId = req.RequirementID;

        return await context.ProjectGroups
            .Include(g => g.Project)
            .Include(g => g.GroupMembers)
                .ThenInclude(m => m.User)
            .Where(g => g.Project.SemesterID == semesterId)
            .Where(g => !context.Submissions.Any(s => s.GroupID == g.GroupID && s.RequirementID == requirementId))
            .SelectMany(g => g.GroupMembers.Select(m => m.User))
            .ToListAsync();
    }

    private async Task<int> SendDeadlineNotificationsIfMissingAsync(GpmsDbContext context, IEmailService emailService, SubmissionRequirement req, List<User> members, string type, CancellationToken stoppingToken)
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
            var reqId = req.RequirementID;
            bool alreadyNotified = await context.Notifications
                .AnyAsync(n => n.RecipientID == member.UserID 
                            && n.Type == NotificationType.Deadline 
                            && n.RelatedEntityID == reqId
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

                await SendEmailSafelyAsync(emailService, member, title, content, $"<strong>Original Deadline:</strong> {req.Deadline:yyyy-MM-dd HH:mm}");
                count++;
            }
        }
        return count;
    }

    #endregion

    #region Review Session Reminders

    private async Task<int> ProcessReviewSessionRemindersAsync(GpmsDbContext context, IEmailService emailService, CancellationToken stoppingToken)
    {
        var today = DateTime.UtcNow.Date;
        int count = 0;

        var sessions = await context.ReviewSessions
            .Include(s => s.ReviewRound)
            .Include(s => s.Group)
                .ThenInclude(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
            .Where(s => s.ScheduledAt.Date >= today && s.ScheduledAt.Date <= today.AddDays(3))
            .ToListAsync(stoppingToken);

        foreach (var session in sessions)
        {
            var daysUntil = (session.ScheduledAt.Date - today).Days;
            var members = session.Group.GroupMembers.Select(m => m.User).ToList();

            if (members.Any())
            {
                if (daysUntil <= 1)
                {
                    count += await SendSessionNotificationsIfMissingAsync(context, emailService, session, members, "Urgent", stoppingToken);
                }
                else if (daysUntil <= 3)
                {
                    count += await SendSessionNotificationsIfMissingAsync(context, emailService, session, members, "Upcoming", stoppingToken);
                }
            }
        }
        return count;
    }

    private async Task<int> SendSessionNotificationsIfMissingAsync(GpmsDbContext context, IEmailService emailService, ReviewSessionInfo session, List<User> members, string type, CancellationToken stoppingToken)
    {
        int count = 0;
        string title;
        string content;

        if (type == "Urgent")
        {
            title = $"Urgent: Round {session.ReviewRound.RoundNumber} Review Session Tomorrow";
            content = $"Your Round {session.ReviewRound.RoundNumber} review session for group '{session.Group.GroupName}' is scheduled for tomorrow at {session.ScheduledAt:HH:mm}.";
        }
        else
        {
            title = $"Upcoming Round {session.ReviewRound.RoundNumber} Review Session";
            content = $"Your Round {session.ReviewRound.RoundNumber} review session for group '{session.Group.GroupName}' is scheduled for {session.ScheduledAt:yyyy-MM-dd} at {session.ScheduledAt:HH:mm}.";
        }

        foreach (var member in members)
        {
            var sessionId = session.SessionID;
            bool alreadyNotified = await context.Notifications
                .AnyAsync(n => n.RecipientID == member.UserID 
                            && n.Type == NotificationType.Schedule 
                            && n.RelatedEntityType == "ReviewSession"
                            && n.RelatedEntityID == sessionId
                            && (type == "Urgent" ? n.Title.Contains("Urgent") : n.Title.Contains("Upcoming")), stoppingToken);

            if (!alreadyNotified)
            {
                context.Notifications.Add(new Notification
                {
                    RecipientID = member.UserID,
                    Title = title,
                    Content = content,
                    Type = NotificationType.Schedule,
                    RelatedEntityType = "ReviewSession",
                    RelatedEntityID = session.SessionID,
                    CreatedAt = DateTime.UtcNow
                });

                string sessionDetails = $"<strong>Scheduled At:</strong> {session.ScheduledAt:yyyy-MM-dd HH:mm}";

                await SendEmailSafelyAsync(emailService, member, title, content, sessionDetails);
                count++;
            }
        }
        return count;
    }

    #endregion

    private async Task SendEmailSafelyAsync(IEmailService emailService, User member, string title, string content, string extraDetails)
    {
        if (string.IsNullOrEmpty(member.Email)) return;

        try
        {
            var subject = $"[GPMS] {title}";
            var body = $@"
                <h2>{title}</h2>
                <p>Hello {member.FullName},</p>
                <p>{content}</p>
                <p>{extraDetails}</p>
                <br/>
                <p>This is an automated message from the Graduation Project Management System.</p>";

            await emailService.SendEmailAsync(member.Email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {Email}", member.Email);
        }
    }
}
