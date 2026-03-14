using GPMS.Application.DTOs;
using System.Collections.Generic;

namespace GPMS.Web.ViewModels;

public class StudentDashboardViewModel
{
    public ProjectDto? Project { get; set; }
    public IEnumerable<SubmissionItemDto> ActiveSubmissions { get; set; } = new List<SubmissionItemDto>();
    public IEnumerable<DashboardFeedbackDto> RecentFeedbacks { get; set; } = new List<DashboardFeedbackDto>();
}
