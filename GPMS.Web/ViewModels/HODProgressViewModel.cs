using GPMS.Application.DTOs;
using System.Collections.Generic;

namespace GPMS.Web.ViewModels;

public class HODProgressViewModel
{
    public int? SemesterID { get; set; }
    public List<SemesterDto> Semesters { get; set; } = new();
    public List<MajorDto> Majors { get; set; } = new();
    public List<GroupProgressItemDto> Groups { get; set; } = new();
    public List<ReviewRoundDto> Rounds { get; set; } = new();
}

public class GroupProgressItemDto
{
    public int GroupID { get; set; }
    public string GroupName { get; set; }
    public string ProjectCode { get; set; }
    public string ProjectName { get; set; }
    public string MajorName { get; set; }
    public string SupervisorName { get; set; }
    public List<RoundProgressStatusDto> RoundStatuses { get; set; } = new();
}

public class RoundProgressStatusDto
{
    public int RoundNumber { get; set; }
    public bool IsSubmitted { get; set; }
    public bool IsApproved { get; set; } // Approved by Mentor
    public bool IsEvaluated { get; set; } // Scored by Reviewer
}
