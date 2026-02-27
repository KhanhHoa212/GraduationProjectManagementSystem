namespace GPMS.Domain.Entities;

public class SubmissionRequirement
{
    public int RequirementID { get; set; }
    public int ReviewRoundID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AllowedFormats { get; set; }
    public int? MaxFileSizeMB { get; set; } = 50;
    public bool IsRequired { get; set; } = true;
    public DateTime Deadline { get; set; }

    // Navigation
    public virtual ReviewRound ReviewRound { get; set; } = null!;
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
