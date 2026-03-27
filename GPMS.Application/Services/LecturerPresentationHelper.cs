using GPMS.Application.DTOs.Lecturer;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;

namespace GPMS.Application.Services;

internal static class LecturerPresentationHelper
{
    public static string ResolveMilestoneStatus(ReviewRound round, Submission? submission, Evaluation? evaluation, MentorRoundReview? mentorGate)
    {
        if (mentorGate?.DecisionStatus == MentorGateStatus.Rejected)
        {
            return "Blocked by mentor";
        }

        if (evaluation?.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Rejected)
        {
            return "Needs reviewer revision";
        }

        if (evaluation?.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Pending)
        {
            return "Waiting for mentor approval";
        }

        if (evaluation?.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Approved)
        {
            return "Approved for student release";
        }

        if (evaluation?.Status == EvaluationStatus.Submitted)
        {
            return "Reviewed";
        }

        if (submission != null)
        {
            return mentorGate?.DecisionStatus == MentorGateStatus.Approved
                ? "Approved for reviewer evaluation"
                : "Waiting for mentor approval";
        }

        if (round.StartDate > DateTime.UtcNow)
        {
            return "Upcoming";
        }

        return "In progress";
    }

    public static string ResolveLocation(ReviewSessionInfo session)
    {
        if (IsOnlineSession(session))
        {
            return "Online session";
        }

        if (session.Room == null)
        {
            return "Location pending";
        }

        return string.IsNullOrWhiteSpace(session.Room.Building)
            ? session.Room.RoomCode
            : $"{session.Room.RoomCode} - {session.Room.Building}";
    }

    public static string ResolveLocation(GroupSessionSummaryDto session)
    {
        if (session.IsOnline)
        {
            return "Online session";
        }

        if (string.IsNullOrWhiteSpace(session.RoomCode))
        {
            return "Location pending";
        }

        return string.IsNullOrWhiteSpace(session.Building)
            ? session.RoomCode
            : $"{session.RoomCode} - {session.Building}";
    }

    public static bool IsOnlineSession(ReviewSessionInfo session)
    {
        return session.ReviewRound?.RoundType == RoundType.Online;
    }

    public static StudentMemberDto MapToStudentMemberDto(GroupMember member) => new()
    {
        UserId = member.UserID,
        FullName = member.User?.FullName ?? "Unknown",
        Email = member.User?.Email,
        RoleInGroup = member.RoleInGroup.ToString(),
        AvatarUrl = BuildAvatarUrl(member.User?.FullName)
    };

    public static string GetNotificationIcon(NotificationType type) => type switch
    {
        NotificationType.Feedback => "chat",
        NotificationType.Schedule => "calendar_today",
        NotificationType.Deadline => "event_busy",
        NotificationType.Review => "assignment",
        _ => "notifications"
    };

    public static string GetNotificationColor(NotificationType type) => type switch
    {
        NotificationType.Feedback => "var(--fpt-orange)",
        NotificationType.Schedule => "#0EA5E9",
        NotificationType.Deadline => "#DC2626",
        NotificationType.Review => "#6B7280",
        _ => "#6B7280"
    };

    public static string? ResolveNotificationUrl(Notification notification)
    {
        return notification.RelatedEntityType switch
        {
            "Feedback" when notification.RelatedEntityID.HasValue => $"/Lecturer/FeedbackApprovalDetail/{notification.RelatedEntityID.Value}",
            _ => "/Lecturer/History"
        };
    }

    private static string BuildAvatarUrl(string? fullName)
    {
        return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(fullName ?? "U")}&background=E5E7EB&color=374151";
    }
}
