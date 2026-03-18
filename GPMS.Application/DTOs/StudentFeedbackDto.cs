using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs;

public class StudentFeedbackDto
{
    public List<FeedbackRoundDto> Rounds { get; set; } = new();
    public int? SelectedRoundId { get; set; }
    public FeedbackSummaryDto? Summary { get; set; }
    public List<FeedbackCriteriaDto> Details { get; set; } = new();
    public string? OverallFeedback { get; set; }
}

public class FeedbackRoundDto
{
    public int ReviewRoundId { get; set; }
    public int RoundNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Completed, Ongoing, Upcoming
    public bool IsSelected { get; set; }
}

public class FeedbackSummaryDto
{
    public decimal TotalScore { get; set; }
    public decimal MaxPossibleScore { get; set; }
    public string StatusText { get; set; } = string.Empty; // e.g., "Approved by Supervisor"
    public DateTime? UpdatedAt { get; set; }
}

public class FeedbackCriteriaDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string? Comment { get; set; }
}
