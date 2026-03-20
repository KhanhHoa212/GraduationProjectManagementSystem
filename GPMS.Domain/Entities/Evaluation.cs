using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class Evaluation
{
    public int EvaluationID { get; set; }
    public int ReviewRoundID { get; set; }
    public string ReviewerID { get; set; } = string.Empty;
    public int GroupID { get; set; }
    public EvaluationStatus Status { get; set; } = EvaluationStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public string? OverallComment { get; set; }

    // Navigation
    public virtual ReviewRound ReviewRound { get; set; } = null!;
    public virtual User Reviewer { get; set; } = null!;
    public virtual ProjectGroup Group { get; set; } = null!;
    public virtual ICollection<EvaluationDetail> EvaluationDetails { get; set; } = new List<EvaluationDetail>();
    public virtual Feedback? Feedback { get; set; }
}
