using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

// -------------------------------------------------------
// My Projects (Mentor)
// -------------------------------------------------------
public class LecturerProjectsDto
{
    public List<LecturerProjectItemDto> Projects { get; set; } = new();
}

public class LecturerProjectItemDto
{
    public int GroupId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public string SupervisorRole { get; set; } = string.Empty;
    public List<string> MemberNames { get; set; } = new();
    public string CurrentRound { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public DateTime? NextSessionAt { get; set; }
    public string? NextSessionLocation { get; set; }
    public int PendingFeedbackCount { get; set; }
}

// -------------------------------------------------------
// Project Group Summary (Projection — read-only)
// Used by Dashboard, Schedule, Projects pages
// -------------------------------------------------------
public class ProjectGroupSummaryDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public List<string> SupervisorIds { get; set; } = new();
    public List<SupervisorRoleDto> SupervisorRoles { get; set; } = new();
    public List<string> MemberNames { get; set; } = new();
    public List<GroupSessionSummaryDto> Sessions { get; set; } = new();
    public List<GroupEvaluationSummaryDto> Evaluations { get; set; } = new();
    public List<GroupSubmissionSummaryDto> Submissions { get; set; } = new();
}

public class SupervisorRoleDto
{
    public string LecturerId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class GroupSessionSummaryDto
{
    public int ReviewRoundId { get; set; }
    public int RoundNumber { get; set; }
    public string RoundType { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? MeetLink { get; set; }
    public string? RoomCode { get; set; }
    public string? Building { get; set; }
}

public class GroupEvaluationSummaryDto
{
    public int EvaluationId { get; set; }
    public int ReviewRoundId { get; set; }
    public string ReviewerId { get; set; } = string.Empty;
    public string? Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? FeedbackId { get; set; }
    public string? ApprovalStatus { get; set; }
}

public class GroupSubmissionSummaryDto
{
    public int RequirementId { get; set; }
    public int ReviewRoundId { get; set; }
    public string? DocumentName { get; set; }
}

