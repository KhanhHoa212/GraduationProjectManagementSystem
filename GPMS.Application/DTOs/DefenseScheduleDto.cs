using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs;

public class DefenseScheduleViewModel
{
    public int ReviewRoundID { get; set; }
    public int RoundNumber { get; set; }
    public string RoundDescription { get; set; }
    public DateTime SelectedDate { get; set; }
    public bool IsFinalRound { get; set; }
    public List<ReviewRoundDto> Rounds { get; set; } = new();
    public List<RoomDto> Rooms { get; set; } = new();
    public List<DefenseScheduleSessionDto> ScheduledSessions { get; set; } = new();
    public List<UnscheduledGroupDto> UnscheduledGroups { get; set; } = new();
    public List<CommitteeDto> AvailableCommittees { get; set; } = new();
}

public class DefenseScheduleSessionDto
{
    public int SessionID { get; set; }
    public int GroupID { get; set; }
    public string ProjectCode { get; set; }
    public string GroupName { get; set; }
    public string ProjectName { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int RoomID { get; set; }
    public string RoomCode { get; set; }
    public string SupervisorName { get; set; }
    public string MeetLink { get; set; }
    public bool IsOnline { get; set; }
    public string DocumentName { get; set; }
    public string SubmissionUrl { get; set; }
    public string MajorName { get; set; }
    public bool IsCommitteeFull => Reviewers.Count >= 3;
    public List<SessionReviewerDto> Reviewers { get; set; } = new();
}

public class SessionReviewerDto
{
    public string ReviewerID { get; set; }
    public string ReviewerName { get; set; }
    public CommitteeRole Role { get; set; }
}

public class RoomDto
{
    public int RoomID { get; set; }
    public string RoomCode { get; set; }
    public int Capacity { get; set; }
    public string Building { get; set; }
}

public class UnscheduledGroupDto
{
    public int GroupID { get; set; }
    public string ProjectCode { get; set; }
    public string GroupName { get; set; }
    public string ProjectName { get; set; }
    public string MajorName { get; set; }
    public bool IsReady { get; set; }
    public double PlagiarismScore { get; set; }
    public bool IsApprovedBySupervisor { get; set; }
    public string SupervisorName { get; set; }
    public List<SessionReviewerDto> Reviewers { get; set; } = new();
}
