using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class MentorRoundReview
{
    public int ReviewRoundID { get; set; }
    public int GroupID { get; set; }
    public string SupervisorID { get; set; } = string.Empty;
    public MentorGateStatus DecisionStatus { get; set; } = MentorGateStatus.Pending;
    public string? ProgressComment { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ReviewerNotifiedAt { get; set; }

    // Navigation
    public virtual ReviewRound ReviewRound { get; set; } = null!;
    public virtual ProjectGroup Group { get; set; } = null!;
    public virtual User Supervisor { get; set; } = null!;
}
