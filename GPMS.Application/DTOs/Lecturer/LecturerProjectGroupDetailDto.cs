using System;
using System.Collections.Generic;
using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs.Lecturer;

// -------------------------------------------------------
// Project Group Detail
// -------------------------------------------------------
public class LecturerProjectGroupDetailDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public int? PendingFeedbackId { get; set; }
    public List<StudentMemberDto> Members { get; set; } = new();
    public List<MilestoneDetailDto> Milestones { get; set; } = new();
    public MeetingScheduleDto? NextMeeting { get; set; }
}

public class StudentMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string RoleInGroup { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}

public class MilestoneDetailDto
{
    public int RoundId { get; set; }
    public int RoundNumber { get; set; }
    public int? SubmissionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RoundType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = "PENDING";
    public MentorGateStatus MentorGateStatus { get; set; } = MentorGateStatus.Pending;
    public string? MentorGateComment { get; set; }
    public string? ReportDocumentUrl { get; set; }
    public string? SlideDocumentUrl { get; set; }
    public string? SourceCodeUrl { get; set; }
    public string? SubmissionFileName { get; set; }
    public decimal? SubmissionSizeMb { get; set; }
    public string? SubmittedByName { get; set; }
    public string? ReviewerName { get; set; }
    public string? FeedbackStatus { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? Location { get; set; }
}

public class MeetingScheduleDto
{
    public DateTime ScheduledAt { get; set; }
    public bool IsOnline { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
