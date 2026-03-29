namespace GPMS.Domain.Entities;

public class ReviewerAssignment
{
    public int AssignmentID { get; set; }
    public int ReviewRoundID { get; set; }
    public int GroupID { get; set; }
    public string ReviewerID { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsRandom { get; set; } = true;
    public string? AssignedBy { get; set; }
    public int? CommitteeRole { get; set; } // 1: Chairperson, 2: Secretary, 3: Reviewer
    public int? CommitteeID { get; set; }

    // Navigation
    public virtual Committee? Committee { get; set; }
    public virtual ReviewRound ReviewRound { get; set; } = null!;
    public virtual ProjectGroup Group { get; set; } = null!;
    public virtual User Reviewer { get; set; } = null!;
    public virtual User? AdminWhoAssigned { get; set; }
}
