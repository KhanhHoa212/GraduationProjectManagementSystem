namespace GPMS.Domain.Entities;

public class EvaluationDetail
{
    public int EvaluationID { get; set; }
    public int ItemID { get; set; }
    public decimal Score { get; set; }
    public string? AssessmentValue { get; set; }
    public string? Comment { get; set; }
    public string? MentorComment { get; set; }
    public string? GradeDescription { get; set; }

    // Navigation
    public virtual Evaluation Evaluation { get; set; } = null!;
    public virtual ChecklistItem Item { get; set; } = null!;
}
