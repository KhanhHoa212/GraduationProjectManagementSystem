using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs;

// Used in Projects list view
public class ProjectDto
{
    public int ProjectID { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? MajorName { get; set; }
    public int SemesterID { get; set; }
    public string? SemesterCode { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Supervisor info (main supervisor only)
    public string? SupervisorID { get; set; }
    public string? SupervisorName { get; set; }

    // Group count
    public int GroupCount { get; set; }

    // Team members
    public List<ProjectMemberDto> Members { get; set; } = new();
    public string? StudentName { get; set; }
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

    // Submissions
    public List<SubmissionDto> Submissions { get; set; } = new();

    // Review Rounds for the semester
    public List<ReviewRoundDto> ReviewRounds { get; set; } = new();
}

public class SubmissionDto
{
    public int SubmissionID { get; set; }
    public int RequirementID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public SubmissionStatus Status { get; set; }
    public int Version { get; set; }
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
    public string ProjectCode { get; set; } = string.Empty;
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

public class SupervisorAssignmentDto
{
    public List<ProjectDto> UnassignedProjects { get; set; } = new();
    public List<LecturerWorkloadDto> Lecturers { get; set; } = new();
    public List<ProjectDto> AssignedProjects { get; set; } = new();
}

public class LecturerWorkloadDto
{
    public string LecturerID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Level { get; set; }
    public string? Specialty { get; set; }
    public int CurrentWorkload { get; set; }
    public int MaxWorkload { get; set; } = 5;
}
