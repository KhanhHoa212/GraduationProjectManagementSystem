namespace GPMS.Application.DTOs;

public class HODReportDto
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
    public List<MajorDistributionDto> MajorDistribution { get; set; } = new();

    // --- Submission Stats per Review Round ---
    public List<RoundSubmissionStatDto> RoundSubmissionStats { get; set; } = new();

    // --- Supervisor Workload ---
    public List<SupervisorWorkloadDto> SupervisorWorkloads { get; set; } = new();

    // --- Mentor Decision Stats per Round ---
    public List<RoundMentorDecisionStatDto> RoundMentorStats { get; set; } = new();
}

public class MajorDistributionDto
{
    public string MajorName { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
}

public class RoundSubmissionStatDto
{
    public int RoundNumber { get; set; }
    public string RoundDescription { get; set; } = string.Empty;
    public int TotalRequired { get; set; }
    public int OnTimeCount { get; set; }
    public int LateCount { get; set; }
    public int NotSubmittedCount { get; set; }
}

public class SupervisorWorkloadDto
{
    public string LecturerName { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
    public int GroupCount { get; set; }
    public int StudentCount { get; set; }
}

public class RoundMentorDecisionStatDto
{
    public int RoundNumber { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
    public int PendingCount { get; set; }
    public int StoppedCount { get; set; }
}
