namespace GPMS.Domain.Entities;

public class ReviewChecklist
{
    public int ChecklistID { get; set; }
    public int ReviewRoundID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ReviewRound ReviewRound { get; set; } = null!;
    public virtual User? Creator { get; set; }
    public virtual ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();
}
