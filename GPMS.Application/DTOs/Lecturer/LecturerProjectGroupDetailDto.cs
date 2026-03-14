using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

// -------------------------------------------------------
// Project Group Detail
// -------------------------------------------------------
public class LecturerProjectGroupDetailDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public int? PendingFeedbackId { get; set; }
    public List<StudentMemberDto> Members { get; set; } = new();
    public MilestoneDetailDto Round1Milestone { get; set; } = new();
    public MilestoneDetailDto Round2Milestone { get; set; } = new();
    public MilestoneDetailDto DefenseMilestone { get; set; } = new();
}

public class StudentMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleInGroup { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}

public class MilestoneDetailDto
{
    public int RoundId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? ReportDocumentUrl { get; set; }
    public string? SlideDocumentUrl { get; set; }
    public string? SourceCodeUrl { get; set; }
}
