using System.Collections.Generic;

namespace GPMS.Domain.Entities;

public class Committee
{
    public int CommitteeID { get; set; }
    public string CommitteeName { get; set; } = string.Empty;
    public int SemesterID { get; set; }
    
    // Core members usually for the whole session
    public string ChairpersonID { get; set; } = string.Empty;
    public string SecretaryID { get; set; } = string.Empty;
    public string ReviewerID { get; set; } = string.Empty; // Reviewer 1
    public string? Reviewer2ID { get; set; } // Reviewer 2 (Optional)
    public string? Reviewer3ID { get; set; } // Reviewer 3 (Optional)

    // Navigation
    public virtual Semester Semester { get; set; } = null!;
    public virtual User Chairperson { get; set; } = null!;
    public virtual User Secretary { get; set; } = null!;
    public virtual User Reviewer { get; set; } = null!;
    public virtual User? Reviewer2 { get; set; }
    public virtual User? Reviewer3 { get; set; }

    // Reverse Navigation to group assignments
    public virtual ICollection<ReviewerAssignment> ReviewerAssignments { get; set; } = new List<ReviewerAssignment>();
}
