using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class Submission
{
    public int SubmissionID { get; set; }
    public int RequirementID { get; set; }
    public int GroupID { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public decimal? FileSizeMB { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string SubmittedBy { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; }
    public int Version { get; set; } = 1;

    // Navigation
    public virtual SubmissionRequirement Requirement { get; set; } = null!;
    public virtual ProjectGroup Group { get; set; } = null!;
    public virtual User Submitter { get; set; } = null!;
}
