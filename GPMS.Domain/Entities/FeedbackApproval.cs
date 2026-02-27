using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class FeedbackApproval
{
    public int FeedbackID { get; set; }
    public string? SupervisorID { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public string? SupervisorComment { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? AutoReleasedAt { get; set; }
    public bool IsVisibleToStudent { get; set; } = false;

    // Navigation
    public virtual Feedback Feedback { get; set; } = null!;
    public virtual User? Supervisor { get; set; }
}
