using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

// -------------------------------------------------------
// My Projects (Mentor)
// -------------------------------------------------------
public class LecturerProjectsDto
{
    public List<LecturerProjectItemDto> Projects { get; set; } = new();
}

public class LecturerProjectItemDto
{
    public int GroupId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public string SupervisorRole { get; set; } = string.Empty;
    public List<string> MemberNames { get; set; } = new();
    public string CurrentRound { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
}
