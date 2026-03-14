using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

// -------------------------------------------------------
// Feedback Approvals (Mentor)
// -------------------------------------------------------
public class LecturerFeedbackApprovalsDto
{
    public List<PendingFeedbackItemDto> PendingFeedbacks { get; set; } = new();
}

public class PendingFeedbackItemDto
{
    public int FeedbackId { get; set; }
    public int EvaluationId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ReviewRoundName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime AutoReleaseAt { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
}
