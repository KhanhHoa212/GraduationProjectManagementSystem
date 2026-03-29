using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class ReminderService : IReminderService
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IProjectGroupRepository _projectGroupRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IReviewSessionRepository _reviewSessionRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        ISubmissionRepository submissionRepository,
        IProjectGroupRepository projectGroupRepository,
        INotificationRepository notificationRepository,
        IReviewSessionRepository reviewSessionRepository,
        IEmailService emailService,
        ILogger<ReminderService> logger)
    {
        _submissionRepository = submissionRepository;
        _projectGroupRepository = projectGroupRepository;
        _notificationRepository = notificationRepository;
        _reviewSessionRepository = reviewSessionRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<int> ProcessAllRemindersAsync(CancellationToken cancellationToken = default)
    {
        int deadlineNotifications = await ProcessDeadlineRemindersAsync(cancellationToken);
        int sessionNotifications = await ProcessReviewSessionRemindersAsync(cancellationToken);

        if (deadlineNotifications > 0 || sessionNotifications > 0)
        {
            await _notificationRepository.SaveChangesAsync();
            _logger.LogInformation("Successfully processed reminders. Created {DeadlineCount} deadline and {SessionCount} session notifications.", deadlineNotifications, sessionNotifications);
        }

        return deadlineNotifications + sessionNotifications;
    }

    #region Deadline Reminders

    private async Task<int> ProcessDeadlineRemindersAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        int count = 0;

        var requirements = await _submissionRepository.GetActiveRequirementsAsync(cancellationToken);

        foreach (var req in requirements)
        {
            var daysUntil = (req.Deadline.Date - today).Days;
            var members = await _projectGroupRepository.GetUsersMissingRequirementAsync(req.ReviewRound.SemesterID, req.RequirementID, cancellationToken);

            if (members.Any())
            {
                if (daysUntil < 0)
                {
                    count += await SendDeadlineNotificationsIfMissingAsync(req, members, "Overdue", cancellationToken);
                }
                else if (daysUntil <= 1)
                {
                    count += await SendDeadlineNotificationsIfMissingAsync(req, members, "Urgent", cancellationToken);
                }
                else if (daysUntil <= 3)
                {
                    count += await SendDeadlineNotificationsIfMissingAsync(req, members, "Upcoming", cancellationToken);
                }
            }
        }
        return count;
    }

    private async Task<int> SendDeadlineNotificationsIfMissingAsync(
        SubmissionRequirement req, 
        List<User> members, 
        string type, 
        CancellationToken cancellationToken)
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
            bool alreadyNotified = await _notificationRepository.HasNotificationAsync(
                member.UserID, 
                NotificationType.Deadline, 
                req.RequirementID, 
                type == "Overdue" ? "Overdue" : type == "Urgent" ? "Urgent" : "Upcoming",
                cancellationToken);

            if (!alreadyNotified)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    RecipientID = member.UserID,
                    Title = title,
                    Content = content,
                    Type = NotificationType.Deadline,
                    RelatedEntityType = "SubmissionRequirement",
                    RelatedEntityID = req.RequirementID,
                    CreatedAt = DateTime.UtcNow
                });

                await SendEmailSafelyAsync(member, title, content, $"<strong>Original Deadline:</strong> {req.Deadline:yyyy-MM-dd HH:mm}");
                count++;
            }
        }
        return count;
    }

    #endregion

    #region Review Session Reminders

    private async Task<int> ProcessReviewSessionRemindersAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        int count = 0;

        var sessions = await _reviewSessionRepository.GetUpcomingSessionsAsync(3, cancellationToken);

        foreach (var session in sessions)
        {
            var daysUntil = (session.ScheduledAt.Date - today).Days;
            var members = session.Group.GroupMembers.Select(m => m.User).ToList();

            if (members.Any())
            {
                if (daysUntil <= 1)
                {
                    count += await SendSessionNotificationsIfMissingAsync(session, members, "Urgent", cancellationToken);
                }
                else if (daysUntil <= 3)
                {
                    count += await SendSessionNotificationsIfMissingAsync(session, members, "Upcoming", cancellationToken);
                }
            }
        }
        return count;
    }

    private async Task<int> SendSessionNotificationsIfMissingAsync(
        ReviewSessionInfo session, 
        List<User> members, 
        string type, 
        CancellationToken cancellationToken)
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
            bool alreadyNotified = await _notificationRepository.HasSessionNotificationAsync(
                member.UserID, 
                session.SessionID, 
                type == "Urgent" ? "Urgent" : "Upcoming", 
                cancellationToken);

            if (!alreadyNotified)
            {
                await _notificationRepository.AddAsync(new Notification
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

                await SendEmailSafelyAsync(member, title, content, sessionDetails);
                count++;
            }
        }
        return count;
    }

    #endregion

    private async Task SendEmailSafelyAsync(User member, string title, string content, string extraDetails)
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

            await _emailService.SendEmailAsync(member.Email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {Email}", member.Email);
        }
    }
}
