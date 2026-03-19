using System;
using System.Collections.Generic;
using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs.Lecturer;

// -------------------------------------------------------
// Evaluation Form (Reviewer)
// -------------------------------------------------------
public class LecturerEvaluationFormDto
{
    public int AssignmentId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public string ReviewRoundName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public List<StudentMemberDto> Members { get; set; } = new();
    public List<ChecklistItemDto> ChecklistItems { get; set; } = new();
    public int? ExistingEvaluationId { get; set; }
    public string? ExistingFeedbackContent { get; set; }
    public List<ExistingScoreDto> ExistingScores { get; set; } = new();
    public ApprovalStatus? FeedbackApprovalStatus { get; set; }
    public string? SupervisorComment { get; set; }
    public MentorGateStatus MentorGateStatus { get; set; } = MentorGateStatus.Pending;
    public string? MentorGateComment { get; set; }
    public bool CanEdit { get; set; } = true;
}

public class ChecklistItemDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string ItemContent { get; set; } = string.Empty;
    public string? SectionCode { get; set; }
    public string? SectionTitle { get; set; }
    public string? PriorityLabel { get; set; }
    public ChecklistInputType InputType { get; set; } = ChecklistInputType.NumericScore;
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; }
    public string? ExcellentRubric { get; set; }
    public string? GoodRubric { get; set; }
    public string? AcceptableRubric { get; set; }
    public string? FailRubric { get; set; }
}

public class ExistingScoreDto
{
    public int ItemId { get; set; }
    public decimal Score { get; set; }
    public string? AssessmentValue { get; set; }
    public string? Comment { get; set; }
    public string? MentorComment { get; set; }
    public string? GradeDescription { get; set; }
}

// -------------------------------------------------------
// Evaluation Form POST model (from Web layer — submitted by form)
// -------------------------------------------------------
public class EvaluationSubmitDto
{
    public int AssignmentId { get; set; }
    public List<ScoreInputDto> CriteriaScores { get; set; } = new();
    public string OverallFeedback { get; set; } = string.Empty;
}

public class ScoreInputDto
{
    public int CriteriaId { get; set; }
    public decimal Score { get; set; }
    public string? AssessmentValue { get; set; }
    public string? Comment { get; set; }
}
