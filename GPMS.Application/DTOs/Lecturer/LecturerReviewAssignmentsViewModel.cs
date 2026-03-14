using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

public class LecturerReviewAssignmentsViewModel
{
    public List<ReviewAssignmentItem> Assignments { get; set; } = new();
    public int PendingEvaluationsCount { get; set; }
    public int ScheduledTodayCount { get; set; }
    public int CompletedReviewsCount { get; set; }
}

public class ReviewAssignmentItem
{
    public int AssignmentId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ReviewRoundName { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public DateTime ScheduleTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Location { get; set; } = string.Empty; // Room code or Meet link
    public string Venue { get; set; } = string.Empty;
    public bool IsSubmittedByStudent { get; set; }
    public int? EvaluationId { get; set; }
    public bool IsOnline { get; set; }
    public string EvaluationStatus { get; set; } = "Pending"; // Pending, Draft, Submitted
}
