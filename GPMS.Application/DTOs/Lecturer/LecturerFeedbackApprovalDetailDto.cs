using System;
using System.Collections.Generic;

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
    public List<EvaluationScoreItemDto> Scores { get; set; } = new();
    public decimal TotalScore { get; set; }
    public decimal MaxTotalScore { get; set; } = 10.0m;
    public List<StudentMemberDto> Members { get; set; } = new();
}

public class EvaluationScoreItemDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string CriteriaName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public decimal WeightPercentage { get; set; }
    public decimal WeightedScore => Score * (WeightPercentage / 100);
}
