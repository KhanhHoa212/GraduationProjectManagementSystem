using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

public class LecturerProjectGroupDetailViewModel
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    
    // Pending feedback for this group (if any) — used for "View Feedback" button
    public int? PendingFeedbackId { get; set; }
    
    // Derived from GroupMembers
    public List<StudentMemberInfo> Members { get; set; } = new();


    // Relevant Submission Details mapped from ReviewRound -> Submissions -> etc.
    public MilestoneDetail Round1Milestone { get; set; } = new();
    public MilestoneDetail Round2Milestone { get; set; } = new();
    public MilestoneDetail DefenseMilestone { get; set; } = new();
}

public class StudentMemberInfo
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleInGroup { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}

public class MilestoneDetail
{
    public int RoundId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, ON TIME, LATE
    
    // For document links
    public string? ReportDocumentUrl { get; set; }
    public string? SlideDocumentUrl { get; set; }
    public string? SourceCodeUrl { get; set; }
}
