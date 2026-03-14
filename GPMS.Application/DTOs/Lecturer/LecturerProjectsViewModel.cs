using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

public class LecturerProjectsViewModel
{
    public List<LecturerProjectItem> Projects { get; set; } = new();
}

public class LecturerProjectItem
{
    public int GroupId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public string SupervisorRole { get; set; } = string.Empty; // Main or Co
    public List<string> MemberNames { get; set; } = new();
    public string CurrentRound { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
