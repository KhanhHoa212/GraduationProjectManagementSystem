using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

public class LecturerFeedbackApprovalDetailViewModel
{
    public int FeedbackId { get; set; }
    public int EvaluationId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ReviewRoundName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public string FeedbackContent { get; set; } = string.Empty; // Editable by Mentor
    
    public int CurrentRoundIndex { get; set; }
    public int TotalRounds { get; set; }
    public string GroupIdString { get; set; } = string.Empty;
    
    // Scores from EvaluationDetails
    public List<EvaluationScoreItem> Scores { get; set; } = new();
    public decimal TotalScore { get; set; }
    public decimal MaxTotalScore { get; set; } = 10.0m;
    
    // Group Members Sidebar
    public List<StudentMemberInfo> Members { get; set; } = new();
}

public class EvaluationScoreItem
{
    public string CriteriaName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public decimal WeightPercentage { get; set; }
    public decimal WeightedScore => Score * (WeightPercentage / 100);
}
