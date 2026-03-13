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
    private readonly IReviewRoundService _reviewRoundService;

    public HODController(IProjectService projectService, ISemesterService semesterService, IReviewRoundService reviewRoundService)
    {
        _projectService = projectService;
        _semesterService = semesterService;
        _reviewRoundService = reviewRoundService;
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
            ? (await _projectService.GetProjectsBySemesterAsync(activeSemester.SemesterID)).ToList()
            : (await _projectService.GetAllProjectsAsync()).ToList();

        ViewBag.ActiveCount = projects.Count(p => p.Status == ProjectStatus.Active);
        ViewBag.DraftCount = projects.Count(p => p.Status == ProjectStatus.Draft);
        ViewBag.CompletedCount = projects.Count(p => p.Status == ProjectStatus.Completed);

        return View(projects.OrderByDescending(p => p.CreatedAt).Take(5));
    }

    public async Task<IActionResult> Projects(int? semesterId, string? status, string? search, string? majorName)
    {
        var semesters = (await _semesterService.GetAllSemestersAsync()).ToList();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);

        int? targetSemesterId;
        if (semesterId == -1)
        {
            targetSemesterId = null; 
        }
        else if (semesterId == null)
        {
            targetSemesterId = activeSemester?.SemesterID;
        }
        else
        {
            targetSemesterId = semesterId;
        }

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

        // Filter by major
        if (!string.IsNullOrWhiteSpace(majorName))
            projects = projects.Where(p => p.MajorName == majorName);

        ViewBag.Semesters = semesters;
        ViewBag.SelectedSemesterId = semesterId == -1 ? -1 : targetSemesterId;
        ViewBag.ActiveSemester = activeSemester;
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentMajor = majorName;

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
    public async Task<IActionResult> ReviewRounds()
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);
        
        if (activeSemester == null)
            return View(new List<ReviewRoundDto>());

        var rounds = await _reviewRoundService.GetReviewRoundsBySemesterAsync(activeSemester.SemesterID);
        return View(rounds);
    }

    [HttpGet]
    public async Task<IActionResult> CreateReviewRound()
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);
        ViewBag.ActiveSemester = activeSemester;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReviewRound(CreateReviewRoundDto dto)
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);
        
        // 1. Basic Date Range Validation
        if (dto.StartDate >= dto.EndDate)
        {
            ModelState.AddModelError("EndDate", "End date must be greater than start date.");
        }

        // 2. Future Date Validation
        if (dto.EndDate <= DateTime.Now)
        {
            ModelState.AddModelError("EndDate", "Review round cannot end in the past.");
        }

        // 3. Sequential Validation
        if (activeSemester != null)
        {
            var existingRounds = (await _reviewRoundService.GetReviewRoundsBySemesterAsync(activeSemester.SemesterID))
                                 .OrderBy(r => r.RoundNumber).ToList();

            // Check previous round
            var prevRound = existingRounds.LastOrDefault(r => r.RoundNumber < dto.RoundNumber);
            if (prevRound != null && dto.StartDate < prevRound.EndDate)
            {
                ModelState.AddModelError("StartDate", $"Round {dto.RoundNumber} must start after Round {prevRound.RoundNumber} ends ({prevRound.EndDate:dd/MM/yyyy HH:mm}).");
            }

            // Check next round
            var nextRound = existingRounds.FirstOrDefault(r => r.RoundNumber > dto.RoundNumber);
            if (nextRound != null && dto.EndDate > nextRound.StartDate)
            {
                ModelState.AddModelError("EndDate", $"Round {dto.RoundNumber} must end before Round {nextRound.RoundNumber} starts ({nextRound.StartDate:dd/MM/yyyy HH:mm}).");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ActiveSemester = activeSemester;
            return View(dto);
        }

        await _reviewRoundService.CreateReviewRoundAsync(dto);
        TempData["SuccessMessage"] = "Review round created successfully.";
        return RedirectToAction(nameof(ReviewRounds));
    }

    [HttpGet]
    public async Task<IActionResult> EditReviewRound(int id)
    {
        var round = await _reviewRoundService.GetReviewRoundByIdAsync(id);
        if (round == null) return NotFound();
        
        var dto = new CreateReviewRoundDto
        {
            SemesterID = round.SemesterID,
            RoundNumber = round.RoundNumber,
            RoundType = round.RoundType,
            StartDate = round.StartDate,
            EndDate = round.EndDate,
            SubmissionDeadline = round.SubmissionDeadline,
            Description = round.Description,
            Status = round.Status,
            SubmissionRequirements = round.SubmissionRequirements?.Select(req => new SubmissionRequirementDto
            {
                RequirementID = req.RequirementID,
                DocumentName = req.DocumentName,
                Description = req.Description,
                AllowedFormats = req.AllowedFormats,
                MaxFileSizeMB = req.MaxFileSizeMB,
                IsRequired = req.IsRequired
            }).ToList() ?? new List<SubmissionRequirementDto>()
        };
        
        var semesters = await _semesterService.GetAllSemestersAsync();
        ViewBag.ActiveSemester = semesters.FirstOrDefault(s => s.SemesterID == round.SemesterID);
        ViewBag.ReviewRoundID = id;

        return View("CreateReviewRound", dto); 
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReviewRound(int id, CreateReviewRoundDto dto)
    {
        // 1. Basic Date Range Validation
        if (dto.StartDate >= dto.EndDate)
        {
            ModelState.AddModelError("EndDate", "End date must be greater than start date.");
        }

        // 2. Future Date Validation
        var now = DateTime.Now;
        if (dto.EndDate <= now)
        {
            ModelState.AddModelError("EndDate", "Review round cannot end in the past.");
        }

        // 3. Sequential Validation
        var existingRounds = (await _reviewRoundService.GetReviewRoundsBySemesterAsync(dto.SemesterID))
                             .Where(r => r.ReviewRoundID != id) // Exclude current round being edited
                             .OrderBy(r => r.RoundNumber).ToList();

        // Check previous round
        var prevRound = existingRounds.LastOrDefault(r => r.RoundNumber < dto.RoundNumber);
        if (prevRound != null && dto.StartDate < prevRound.EndDate)
        {
            ModelState.AddModelError("StartDate", $"Round {dto.RoundNumber} must start after Round {prevRound.RoundNumber} ends ({prevRound.EndDate:dd/MM/yyyy HH:mm}).");
        }

        // Check next round
        var nextRound = existingRounds.FirstOrDefault(r => r.RoundNumber > dto.RoundNumber);
        if (nextRound != null && dto.EndDate > nextRound.StartDate)
        {
            ModelState.AddModelError("EndDate", $"Round {dto.RoundNumber} must end before Round {nextRound.RoundNumber} starts ({nextRound.StartDate:dd/MM/yyyy HH:mm}).");
        }

        if (!ModelState.IsValid)
        {
            var semesters = await _semesterService.GetAllSemestersAsync();
            ViewBag.ActiveSemester = semesters.FirstOrDefault(s => s.SemesterID == dto.SemesterID);
            ViewBag.ReviewRoundID = id;
            return View("CreateReviewRound", dto);
        }

        await _reviewRoundService.UpdateReviewRoundAsync(id, dto);
        TempData["SuccessMessage"] = "Review round updated successfully.";
        return RedirectToAction(nameof(ReviewRounds));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReviewRound(int id)
    {
        await _reviewRoundService.DeleteReviewRoundAsync(id);
        TempData["SuccessMessage"] = "Review round deleted successfully.";
        return RedirectToAction(nameof(ReviewRounds));
    }

    public IActionResult Checklists() => View();
    public IActionResult AssignReviewer() => View();
    public IActionResult Reports() => View();
    
    [HttpGet]
    public async Task<IActionResult> CreateProject()
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);
        ViewBag.ActiveSemester = activeSemester;
        return View();
    }
}

