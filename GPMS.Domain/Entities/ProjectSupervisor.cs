using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class ProjectSupervisor
{
    public int ProjectID { get; set; }
    public string LecturerID { get; set; } = string.Empty;
    public ProjectRole Role { get; set; } = ProjectRole.Main;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }

    // Navigation
    public virtual Project Project { get; set; } = null!;
    public virtual User Lecturer { get; set; } = null!;
    public virtual User? AdminWhoAssigned { get; set; }
}
