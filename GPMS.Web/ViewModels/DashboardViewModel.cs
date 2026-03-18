using System;
using System.Collections.Generic;

namespace GPMS.Web.ViewModels;

public class DashboardViewModel
{
    // User counts
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalLecturers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalHODs { get; set; }

    // Semester / projects
    public string CurrentSemesterCode { get; set; } = "N/A";
    public int CurrentSemesterProjectCount { get; set; }

    // System logs (in-memory)
    public List<SystemLogEntry> RecentLogs { get; set; } = new();

    // Chart data (last 7 days)
    public List<DailyVisit> WeeklyVisits { get; set; } = new();
}

public class SystemLogEntry
{
    public string Action { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Success";
}

public class DailyVisit
{
    public string DayLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}
