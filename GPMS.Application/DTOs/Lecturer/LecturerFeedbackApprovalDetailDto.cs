using System;
using System.Collections.Generic;
using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs.Lecturer;

// -------------------------------------------------------
// Feedback Approval Detail (Mentor)
// -------------------------------------------------------
public class LecturerFeedbackApprovalDetailDto
{
    public int FeedbackId { get; set; }
    public int EvaluationId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string ReviewRoundName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public string FeedbackContent { get; set; } = string.Empty;
    public int CurrentRoundIndex { get; set; }
    public int TotalRounds { get; set; }
    public DateTime SubmittedAt { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public string? SupervisorComment { get; set; }
    public MentorGateStatus MentorGateStatus { get; set; } = MentorGateStatus.Pending;
    public string? MentorGateComment { get; set; }
    public List<EvaluationScoreItemDto> Scores { get; set; } = new();
    public List<StudentMemberDto> Members { get; set; } = new();
}

public class EvaluationScoreItemDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string ItemContent { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? ItemType { get; set; }
    public string? Assessment { get; set; }
    public string? ReviewerComment { get; set; }
    public string? MentorComment { get; set; }
    public string? GradeDescription { get; set; }
    public List<RubricDescriptionDto> RubricDescriptions { get; set; } = new();
}


public class FeedbackApprovalDecisionDto
{
    public int FeedbackId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string OverallFeedbackContent { get; set; } = string.Empty;
    public string? SupervisorComment { get; set; }
    public List<MentorChecklistCommentDto> ItemComments { get; set; } = new();
}

public class MentorChecklistCommentDto
{
    public int ItemId { get; set; }
    public string? MentorComment { get; set; }
}
