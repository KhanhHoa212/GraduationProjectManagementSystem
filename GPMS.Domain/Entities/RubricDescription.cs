namespace GPMS.Domain.Entities;

public class RubricDescription
{
    public int RubricID { get; set; }
    public int ItemID { get; set; }
    public string GradeLevel { get; set; } = string.Empty; // 'Excellent'|'Good'|'Acceptable'|'Fail'
    public string Description { get; set; } = string.Empty;

    // Navigation
    public virtual ChecklistItem Item { get; set; } = null!;
}
