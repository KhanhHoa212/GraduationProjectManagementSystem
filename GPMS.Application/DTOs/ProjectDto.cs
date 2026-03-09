using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs;

// Used in Projects list view
public class ProjectDto
{
    public int ProjectID { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? MajorName { get; set; }
    public string? SemesterCode { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Supervisor info (main supervisor only)
    public string? SupervisorID { get; set; }
    public string? SupervisorName { get; set; }

    // Group count
    public int GroupCount { get; set; }
}

// Used in ProjectDetails view
public class ProjectDetailDto
{
    public int ProjectID { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SemesterID { get; set; }
    public string? SemesterCode { get; set; }
    public int MajorID { get; set; }
    public string? MajorName { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Supervisor
    public string? SupervisorID { get; set; }
    public string? SupervisorName { get; set; }

    // Team members (students in all groups under this project)
    public List<ProjectMemberDto> Members { get; set; } = new();
}

public class ProjectMemberDto
{
    public string UserID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public GroupRole RoleInGroup { get; set; }
    public string GroupName { get; set; } = string.Empty;
}

// Create / Update forms
public class CreateProjectDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SemesterID { get; set; }
    public int MajorID { get; set; }
    public ProjectStatus Status { get; set; }
}

public class UpdateProjectDto
{
    public int ProjectID { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
}

public class StudentSearchDto
{
    public string UserID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool AlreadyMember { get; set; }
}
