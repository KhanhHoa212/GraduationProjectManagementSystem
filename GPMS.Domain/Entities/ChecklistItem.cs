namespace GPMS.Domain.Entities;

public class ChecklistItem
{
    public int ItemID { get; set; }
    public int ChecklistID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemContent { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; } = 1.0m;
    public int OrderIndex { get; set; } = 1;

    // Navigation
    public virtual ReviewChecklist Checklist { get; set; } = null!;
    public virtual ICollection<EvaluationDetail> EvaluationDetails { get; set; } = new List<EvaluationDetail>();
}
