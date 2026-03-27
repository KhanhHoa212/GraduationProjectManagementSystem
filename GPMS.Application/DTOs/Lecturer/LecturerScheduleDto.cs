namespace GPMS.Application.DTOs.Lecturer;

public class LecturerScheduleDto
{
    public int TodaySessionsCount { get; set; }
    public int OnlineSessionsCount { get; set; }
    public int OfflineSessionsCount { get; set; }
    public int UpcomingDeadlinesCount { get; set; }
    public int NeedsAttentionCount { get; set; }
    public int WeekSessionsCount { get; set; }
    public string ActiveRoleFilter { get; set; } = "all";
    public string ActiveRangeFilter { get; set; } = "week";
    public int WeekOffset { get; set; }
    public string WeekLabel { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public LecturerScheduleFocusDto? FocusCard { get; set; }
    public List<LecturerScheduleEntryDto> Entries { get; set; } = new();
    public List<LecturerScheduleDayGroupDto> DayGroups { get; set; } = new();
    public List<LecturerScheduleWeekDayDto> WeekDays { get; set; } = new();
    public List<LecturerDeadlineDto> Deadlines { get; set; } = new();
}

public class LecturerScheduleEntryDto
{
    public string RoleKey { get; set; } = "mentor";
    public string RoleLabel { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public string RoundType { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public bool IsOnline { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Guidance { get; set; }
    public string StatusKey { get; set; } = "upcoming";
    public string StatusLabel { get; set; } = "Upcoming";
    public string StatusTone { get; set; } = "info";
    public bool NeedsAttention { get; set; }
    public bool IsToday { get; set; }
    public bool IsPast { get; set; }
    public string TimeHint { get; set; } = string.Empty;
    public string PrimaryActionText { get; set; } = "Open";
    public string PrimaryActionUrl { get; set; } = string.Empty;
    public string? SecondaryActionText { get; set; }
    public string? SecondaryActionUrl { get; set; }
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

public class LecturerScheduleFocusDto
{
    public string Eyebrow { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
    public string? SecondaryActionText { get; set; }
    public string? SecondaryActionUrl { get; set; }
}

public class LecturerScheduleDayGroupDto
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<LecturerScheduleEntryDto> Entries { get; set; } = new();
}

public class LecturerScheduleWeekDayDto
{
    public DateTime Date { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public bool IsToday { get; set; }
    public List<LecturerScheduleEntryDto> Entries { get; set; } = new();
}
