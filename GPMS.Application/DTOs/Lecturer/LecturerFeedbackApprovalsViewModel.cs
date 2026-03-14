using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

public class LecturerFeedbackApprovalsViewModel
{
    public List<PendingFeedbackItem> PendingFeedbacks { get; set; } = new();
}

public class PendingFeedbackItem
{
    public int FeedbackId { get; set; }
    public int EvaluationId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ReviewRoundName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    // How many hours from SubmittedAt until it auto-releases
    public DateTime AutoReleaseAt { get; set; } 
    public string ApprovalStatus { get; set; } = string.Empty;
}
