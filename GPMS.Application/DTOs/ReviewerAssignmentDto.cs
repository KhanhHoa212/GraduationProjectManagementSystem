using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs;

public class ReviewerAssignmentDto
{
    public int ReviewRoundID { get; set; }
    public bool IsFinalRound { get; set; }
    public List<ReviewRoundDto> AllRounds { get; set; } = new();
    public List<UnassignedGroupDto> UnassignedGroups { get; set; } = new();
    public List<AssignedGroupDto> AssignedGroups { get; set; } = new();
    public List<LecturerDto> Lecturers { get; set; } = new();
}

public class UnassignedGroupDto
{
    public int GroupID { get; set; }
    public string ProjectID { get; set; }
    public string ProjectCode { get; set; }
    public string ProjectName { get; set; }
    public string MajorName { get; set; }
    public string GroupName { get; set; }
    public string SupervisorName { get; set; }
    public List<string> MentorIDs { get; set; } = new();
    public List<SessionReviewerDto> Reviewers { get; set; } = new();
}

public class AssignedGroupDto : UnassignedGroupDto
{
}

public class LecturerDto
{
    public string LecturerID { get; set; }
    public string FullName { get; set; }
    public string Level { get; set; }
    public string? Specialty { get; set; }
    public int CurrentWorkload { get; set; }
    public int MaxWorkload { get; set; }
}

public class UpdateReviewerAssignmentRequest
{
    public int ReviewRoundID { get; set; }
    public int GroupID { get; set; }
    public List<ReviewerRoleAssignmentDto> Assignments { get; set; } = new();
}

public class ReviewerRoleAssignmentDto
{
    public string LecturerID { get; set; }
    public CommitteeRole Role { get; set; }
}

public class RemoveReviewerRequest
{
    public int ReviewRoundID { get; set; }
    public int GroupID { get; set; }
    public string LecturerID { get; set; }
}
