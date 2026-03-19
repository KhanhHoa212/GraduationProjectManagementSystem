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
    public decimal TotalScore { get; set; }
    public decimal MaxTotalScore { get; set; } = 10.0m;
    public List<StudentMemberDto> Members { get; set; } = new();
}

public class EvaluationScoreItemDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string CriteriaName { get; set; } = string.Empty;
    public string? SectionCode { get; set; }
    public string? SectionTitle { get; set; }
    public string? PriorityLabel { get; set; }
    public ChecklistInputType InputType { get; set; } = ChecklistInputType.NumericScore;
    public string? AssessmentValue { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public decimal WeightPercentage { get; set; }
    public decimal WeightedScore => Score * (WeightPercentage / 100);
    public string? ReviewerComment { get; set; }
    public string? MentorComment { get; set; }
    public string? GradeDescription { get; set; }
    public string? ExcellentRubric { get; set; }
    public string? GoodRubric { get; set; }
    public string? AcceptableRubric { get; set; }
    public string? FailRubric { get; set; }
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
