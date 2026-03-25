namespace GPMS.Web.ViewModels;

public class HODReportViewModel
{
    // --- KPI Cards ---
    public int TotalProjects { get; set; }
    public int TotalGroups { get; set; }
    public int TotalStudents { get; set; }
    public int TotalSupervisors { get; set; }

    // --- Active Semester Info ---
    public string SemesterCode { get; set; } = "N/A";
    public string AcademicYear { get; set; } = "N/A";

    // --- Project Status Distribution ---
    public int DraftProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int CancelledProjects { get; set; }

    // --- Major Distribution ---
    public List<MajorDistributionItem> MajorDistribution { get; set; } = new();

    // --- Submission Stats per Review Round ---
    public List<RoundSubmissionStat> RoundSubmissionStats { get; set; } = new();

    // --- Supervisor Workload ---
    public Models.PaginatedList<SupervisorWorkloadItem> SupervisorWorkloads { get; set; } = null!;


    // --- Mentor Decision Stats per Round ---
    public List<RoundMentorDecisionStat> RoundMentorStats { get; set; } = new();
}

public class MajorDistributionItem
{
    public string MajorName { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
}

public class RoundSubmissionStat
{
    public int RoundNumber { get; set; }
    public string RoundDescription { get; set; } = string.Empty;
    public int TotalRequired { get; set; }
    public int OnTimeCount { get; set; }
    public int LateCount { get; set; }
    public int NotSubmittedCount { get; set; }
}

public class SupervisorWorkloadItem
{
    public string LecturerName { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
    public int GroupCount { get; set; }
    public int StudentCount { get; set; }
}

public class RoundMentorDecisionStat
{
    public int RoundNumber { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
    public int PendingCount { get; set; }
    public int StoppedCount { get; set; }
}
