namespace GPMS.Domain.Entities;

public class ProjectGroup
{
    public int GroupID { get; set; }
    public int ProjectID { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    public virtual ICollection<ReviewSessionInfo> ReviewSessions { get; set; } = new List<ReviewSessionInfo>();
    public virtual ICollection<ReviewerAssignment> ReviewerAssignments { get; set; } = new List<ReviewerAssignment>();
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    public virtual ICollection<GroupRoundProgress> GroupRoundProgresses { get; set; } = new List<GroupRoundProgress>();
}
