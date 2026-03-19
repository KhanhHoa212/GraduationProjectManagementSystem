using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class User
{
    public string UserID { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserCredential> UserCredentials { get; set; } = new List<UserCredential>();
    public virtual ICollection<LecturerExpertise> LecturerExpertises { get; set; } = new List<LecturerExpertise>();
    public virtual ICollection<ProjectSupervisor> SupervisedProjects { get; set; } = new List<ProjectSupervisor>();
    public virtual ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public virtual ICollection<ReviewerAssignment> ReviewerAssignments { get; set; } = new List<ReviewerAssignment>();
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public virtual ICollection<ReviewChecklist> CreatedChecklists { get; set; } = new List<ReviewChecklist>();
    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    public virtual ICollection<FeedbackApproval> FeedbackApprovals { get; set; } = new List<FeedbackApproval>();
    public virtual ICollection<MentorRoundReview> MentorRoundReviews { get; set; } = new List<MentorRoundReview>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    // Self-referencing navigations for audit fields (AssignedBy)
    public virtual ICollection<ProjectSupervisor> ProjectsAssignedByMe { get; set; } = new List<ProjectSupervisor>();
    public virtual ICollection<ReviewerAssignment> ReviewersAssignedByMe { get; set; } = new List<ReviewerAssignment>();
}
