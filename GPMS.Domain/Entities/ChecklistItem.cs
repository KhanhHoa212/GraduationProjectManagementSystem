using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class ChecklistItem
{
    public int ItemID { get; set; }
    public int ChecklistID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string ItemContent { get; set; } = string.Empty;
    public string? SectionCode { get; set; }
    public string? SectionTitle { get; set; }
    public string? PriorityLabel { get; set; }
    public ChecklistInputType InputType { get; set; } = ChecklistInputType.NumericScore;
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; } = 1.0m;
    public string? ExcellentRubric { get; set; }
    public string? GoodRubric { get; set; }
    public string? AcceptableRubric { get; set; }
    public string? FailRubric { get; set; }
    public int OrderIndex { get; set; } = 1;

    // Navigation
    public virtual ReviewChecklist Checklist { get; set; } = null!;
    public virtual ICollection<EvaluationDetail> EvaluationDetails { get; set; } = new List<EvaluationDetail>();
}
