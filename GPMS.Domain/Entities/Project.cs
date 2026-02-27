using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class Project
{
    public int ProjectID { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SemesterID { get; set; }
    public int MajorID { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Semester Semester { get; set; } = null!;
    public virtual Major Major { get; set; } = null!;
    public virtual ICollection<ProjectSupervisor> ProjectSupervisors { get; set; } = new List<ProjectSupervisor>();
    public virtual ICollection<ProjectGroup> ProjectGroups { get; set; } = new List<ProjectGroup>();
}
