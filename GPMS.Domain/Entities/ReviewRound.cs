using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class ReviewRound
{
    public int ReviewRoundID { get; set; }
    public int SemesterID { get; set; }
    public int RoundNumber { get; set; }
    public RoundType RoundType { get; set; } = RoundType.Online;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmissionDeadline { get; set; }
    public string? Description { get; set; }
    public RoundStatus Status { get; set; } = RoundStatus.Planned;

    // Navigation
    public virtual Semester Semester { get; set; } = null!;
    public virtual ICollection<ReviewSessionInfo> ReviewSessions { get; set; } = new List<ReviewSessionInfo>();
    public virtual ICollection<ReviewerAssignment> ReviewerAssignments { get; set; } = new List<ReviewerAssignment>();
    public virtual ICollection<SubmissionRequirement> SubmissionRequirements { get; set; } = new List<SubmissionRequirement>();
    public virtual ReviewChecklist? ReviewChecklist { get; set; }
    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    public virtual ICollection<GroupRoundProgress> GroupRoundProgresses { get; set; } = new List<GroupRoundProgress>();
}
