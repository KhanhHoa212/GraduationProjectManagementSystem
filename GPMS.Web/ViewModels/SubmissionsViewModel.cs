using GPMS.Application.DTOs;
using System.Collections.Generic;

namespace GPMS.Web.ViewModels;

public class SubmissionsViewModel
{
    public ProjectDto? Project { get; set; }
    public IEnumerable<ReviewRoundDto> ReviewRounds { get; set; } = new List<ReviewRoundDto>();
    public IEnumerable<SubmissionItemDto> Submissions { get; set; } = new List<SubmissionItemDto>();
    public int ActiveRoundNumber { get; set; }
}
