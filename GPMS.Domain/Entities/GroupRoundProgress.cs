using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class GroupRoundProgress
{
    public int GroupID { get; set; }
    public int ReviewRoundID { get; set; }
    public MentorDecision MentorDecision { get; set; } = MentorDecision.Pending;
    public string? MentorComment { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ProjectGroup Group { get; set; } = null!;
    public virtual ReviewRound ReviewRound { get; set; } = null!;
}
