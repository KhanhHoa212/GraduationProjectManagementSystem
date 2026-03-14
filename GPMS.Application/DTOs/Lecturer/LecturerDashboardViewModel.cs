using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

public class LecturerDashboardViewModel
{
    public int MentoringGroupsCount { get; set; }
    public int PendingApprovalsCount { get; set; }
    public int AssignedReviewsCount { get; set; }
    public int UpcomingDeadlinesCount { get; set; }

    public List<DashboardActivityItem> RecentActivities { get; set; } = new();
    public List<DashboardScheduleItem> TodaysSchedule { get; set; } = new();
}

public class DashboardActivityItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = "note_add"; // Material icon name
    public string IconBgColor { get; set; } = "var(--fpt-orange)";
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
}

public class DashboardScheduleItem
{
    public DateTime StartTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public bool IsHighlight { get; set; } // e.g., for orange left border
}
