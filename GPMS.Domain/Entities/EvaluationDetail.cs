namespace GPMS.Domain.Entities;

public class EvaluationDetail
{
    public int EvaluationID { get; set; }
    public int ItemID { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }

    // Navigation
    public virtual Evaluation Evaluation { get; set; } = null!;
    public virtual ChecklistItem Item { get; set; } = null!;
}
