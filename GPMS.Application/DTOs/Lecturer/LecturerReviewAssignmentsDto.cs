using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

// -------------------------------------------------------
// Review Assignments (Reviewer)
// -------------------------------------------------------
public class LecturerReviewAssignmentsDto
{
    public List<ReviewAssignmentItemDto> Assignments { get; set; } = new();
    public int PendingEvaluationsCount { get; set; }
    public int ScheduledTodayCount { get; set; }
    public int CompletedReviewsCount { get; set; }
}

public class ReviewAssignmentItemDto
{
    public int AssignmentId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ReviewRoundName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public string RoundType { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public string? Location { get; set; }
    public bool IsOnline { get; set; }
    public bool HasEvaluation { get; set; }
    public int? EvaluationId { get; set; }
}
