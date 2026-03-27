using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

// -------------------------------------------------------
// Dashboard
// -------------------------------------------------------
public class LecturerDashboardDto
{
    public int MentoringGroupsCount { get; set; }
    public int PendingApprovalsCount { get; set; }
    public int AssignedReviewsCount { get; set; }
    public int UpcomingDeadlinesCount { get; set; }
    public string GuidanceMessage { get; set; } = string.Empty;
    public List<DashboardActivityItemDto> RecentActivities { get; set; } = new();
    public List<DashboardScheduleItemDto> TodaysSchedule { get; set; } = new();
}

public class DashboardActivityItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = "note_add";
    public string IconBgColor { get; set; } = "var(--fpt-orange)";
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
}

public class DashboardScheduleItemDto
{
    public DateTime StartTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public bool IsHighlight { get; set; }
    public string? ActionUrl { get; set; }
}
