using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "HeadOfDept,Admin")]
public class HODController : Controller
{
    private readonly IProjectService _projectService;
    private readonly ISemesterService _semesterService;

    public HODController(IProjectService projectService, ISemesterService semesterService)
    {
        _projectService = projectService;
        _semesterService = semesterService;
    }

    // GET: /HOD/Index (Dashboard)
    public async Task<IActionResult> Index()
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);

        var (total, withGroup, missingSupervisor, missingMembers) =
            await _projectService.GetDashboardStatsAsync(activeSemester?.SemesterID);

        ViewBag.ActiveSemester = activeSemester;
        ViewBag.TotalProjects = total;
        ViewBag.WithGroup = withGroup;
        ViewBag.MissingSupervisor = missingSupervisor;
        ViewBag.MissingMembers = missingMembers;

        // Recent projects for the status table
        var projects = activeSemester != null
            ? await _projectService.GetProjectsBySemesterAsync(activeSemester.SemesterID)
            : await _projectService.GetAllProjectsAsync();

        return View(projects.Take(5));
    }

    // GET: /HOD/Projects
    public async Task<IActionResult> Projects(int? semesterId, string? status, string? search)
    {
        var semesters = (await _semesterService.GetAllSemestersAsync()).ToList();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);

        var targetSemesterId = semesterId ?? activeSemester?.SemesterID;

        IEnumerable<ProjectDto> projects = targetSemesterId.HasValue
            ? await _projectService.GetProjectsBySemesterAsync(targetSemesterId.Value)
            : await _projectService.GetAllProjectsAsync();

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProjectStatus>(status, out var parsedStatus))
            projects = projects.Where(p => p.Status == parsedStatus);

        // Filter by search
        if (!string.IsNullOrWhiteSpace(search))
            projects = projects.Where(p =>
                p.ProjectCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.ProjectName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (p.SupervisorName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

        ViewBag.Semesters = semesters;
        ViewBag.SelectedSemesterId = targetSemesterId;
        ViewBag.ActiveSemester = activeSemester;
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search;

        return View(projects.ToList());
    }

    // GET: /HOD/ProjectDetails/{id}
    public async Task<IActionResult> ProjectDetails(int? id)
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        ViewBag.Semesters = semesters;

        if (id == null)
        {
            // Create mode - return empty view
            return View(new ProjectDetailDto());
        }

        var project = await _projectService.GetProjectDetailAsync(id.Value);
        if (project == null) return NotFound();

        return View(project);
    }

    // POST: /HOD/CreateProject
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProject(CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Projects));

        await _projectService.CreateProjectAsync(dto);
        TempData["SuccessMessage"] = "Project created successfully.";
        return RedirectToAction(nameof(Projects));
    }

    // POST: /HOD/UpdateProject
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProject(UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(ProjectDetails), new { id = dto.ProjectID });

        await _projectService.UpdateProjectAsync(dto);
        TempData["SuccessMessage"] = "Project updated successfully.";
        return RedirectToAction(nameof(ProjectDetails), new { id = dto.ProjectID });
    }

    // ============================================================
    // Member Management (JSON API)
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> SearchStudents(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(new List<object>());

        var results = await _projectService.SearchStudentsAsync(q);
        return Json(results.Select(s => new { s.UserID, s.FullName, s.Email }));
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(int projectId, string userId)
    {
        var (success, message) = await _projectService.AddMemberAsync(projectId, userId);
        return Json(new { success, message });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveMember(int projectId, string userId)
    {
        var success = await _projectService.RemoveMemberAsync(projectId, userId);
        return Json(new { success });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMemberRole(int projectId, string userId, string role)
    {
        var (success, message) = await _projectService.UpdateMemberRoleAsync(projectId, userId, role);
        return Json(new { success, message });
    }

    public IActionResult Groups() => View();
    public IActionResult GroupDetails(string id) => View();
    public IActionResult AssignSupervisor() => View();
    public IActionResult Import() => View();
}

