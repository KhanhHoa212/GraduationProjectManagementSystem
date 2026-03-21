using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class ChecklistItem
{
    public int ItemID { get; set; }
    public int ChecklistID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string ItemContent { get; set; } = string.Empty;
    public string ItemType { get; set; } = "YesNo";
    public string? Section { get; set; }
    public int OrderIndex { get; set; } = 1;

    // Navigation
    public virtual ReviewChecklist Checklist { get; set; } = null!;
    public virtual ICollection<EvaluationDetail> EvaluationDetails { get; set; } = new List<EvaluationDetail>();
    public virtual ICollection<RubricDescription> RubricDescriptions { get; set; } = new List<RubricDescription>();
}
