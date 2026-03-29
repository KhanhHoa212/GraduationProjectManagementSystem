using GPMS.Application.DTOs;
using GPMS.Web.Models;
using System.Collections.Generic;

namespace GPMS.Web.ViewModels;

public class SupervisorAssignmentViewModel
{
    public List<ProjectDto> UnassignedProjects { get; set; } = new();
    public List<LecturerWorkloadDto> Lecturers { get; set; } = new();
    public PaginatedList<ProjectDto> AssignedProjects { get; set; }
    public string? CurrentSearch { get; set; }
    public string? CurrentLecturerFilter { get; set; }
    public int? SelectedSemesterId { get; set; }
}
