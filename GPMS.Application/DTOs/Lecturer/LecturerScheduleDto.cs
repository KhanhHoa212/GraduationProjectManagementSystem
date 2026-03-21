namespace GPMS.Application.DTOs.Lecturer;

public class LecturerScheduleDto
{
    public int TodaySessionsCount { get; set; }
    public int OnlineSessionsCount { get; set; }
    public int OfflineSessionsCount { get; set; }
    public int UpcomingDeadlinesCount { get; set; }
    public List<LecturerScheduleEntryDto> Entries { get; set; } = new();
    public List<LecturerDeadlineDto> Deadlines { get; set; } = new();
}

public class LecturerScheduleEntryDto
{
    public string RoleLabel { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public string RoundType { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public bool IsOnline { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? MeetLink { get; set; }
    public string? Guidance { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
}

public class LecturerDeadlineDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueAt { get; set; }
    public string Severity { get; set; } = "info";
    public string ActionUrl { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
}
