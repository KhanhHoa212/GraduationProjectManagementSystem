namespace GPMS.Domain.Entities;

public class Feedback
{
    public int FeedbackID { get; set; }
    public int EvaluationID { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Evaluation Evaluation { get; set; } = null!;
    public virtual FeedbackApproval? FeedbackApproval { get; set; }
}
