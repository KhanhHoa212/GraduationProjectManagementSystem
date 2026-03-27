using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using GPMS.Web.ViewModels;
namespace GPMS.Web.Controllers;

[Authorize(Roles = "HeadOfDept")]
public class HODController : Controller
{
    private readonly IProjectService _projectService;
    private readonly ISemesterService _semesterService;
    private readonly IReviewRoundService _reviewRoundService;
    private readonly IChecklistService _checklistService;
    private readonly IExcelService _excelService;
    private readonly IReportService _reportService;
    private readonly IMajorService _majorService;
    private readonly IUserService _userService;
    private readonly IRoomRepository _roomRepository;
    private readonly IProjectGroupRepository _groupRepository;

    public HODController(
        IProjectService projectService, 
        ISemesterService semesterService, 
        IReviewRoundService reviewRoundService,
        IChecklistService checklistService,
        IExcelService excelService,
        IReportService reportService,
        IMajorService majorService,
        IUserService userService,
        IRoomRepository roomRepository,
        IProjectGroupRepository groupRepository)
    {
        _projectService = projectService;
        _semesterService = semesterService;
        _reviewRoundService = reviewRoundService;
        _checklistService = checklistService;
        _excelService = excelService;
        _reportService = reportService;
        _majorService = majorService;
        _userService = userService;
        _roomRepository = roomRepository;
        _groupRepository = groupRepository;
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

        // Fetch review rounds for the timeline
        if (activeSemester != null)
        {
            ViewBag.ReviewRounds = await _reviewRoundService.GetReviewRoundsBySemesterAsync(activeSemester.SemesterID);
        }

        return View(projects.OrderByDescending(p => p.CreatedAt).Take(5));
    }

    public async Task<IActionResult> Projects(int? semesterId, string? status, string? search, string? majorName, int page = 1)
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
        ViewBag.Majors = await _majorService.GetAllMajorsAsync();
        ViewBag.SelectedSemesterId = semesterId == -1 ? -1 : targetSemesterId;
        ViewBag.ActiveSemester = activeSemester;
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentMajor = majorName;

        // Use PaginatedList (Assuming pageSize = 10 for HOD Projects)
        var paginatedProjects = Models.PaginatedList<ProjectDto>.Create(projects.OrderByDescending(p => p.CreatedAt), page, 10);
        return View(paginatedProjects);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadProjectTemplate()
    {
        var fileBytes = await _excelService.GenerateProjectImportTemplateAsync();
        if (fileBytes == null || fileBytes.Length == 0)
        {
            TempData["ErrorMessage"] = "Không thể tạo file mẫu. Vui lòng kiểm tra học kỳ hiện tại.";
            return RedirectToAction(nameof(Projects));
        }
        
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GPMS_Project_Import_Template.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> PreviewProjectImport(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn file Excel.";
            return RedirectToAction(nameof(Projects));
        }

        var results = await _excelService.PreviewProjectImportAsync(file);
        return View(results);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmProjectImport(string importDataJson)
    {
        if (string.IsNullOrEmpty(importDataJson))
        {
            TempData["ErrorMessage"] = "Không có dữ liệu để import.";
            return RedirectToAction(nameof(Projects));
        }

        try
        {
            var projects = System.Text.Json.JsonSerializer.Deserialize<List<ProjectImportRowDto>>(importDataJson);
            if (projects == null || !projects.Any())
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ hoặc rỗng.";
                return RedirectToAction(nameof(Projects));
            }

            var activeSemester = await _semesterService.GetCurrentSemesterAsync();
            if (activeSemester == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy học kỳ hoạt động (Semester Status = Active).";
                return RedirectToAction(nameof(Projects));
            }

            var requestedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (successCount, message) = await _projectService.BulkImportProjectsAsync(projects, activeSemester.SemesterID, requestedBy);

            if (successCount > 0)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = "Không có dự án nào được nhập thành công. Vui lòng kiểm tra lại dữ liệu.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình xử lý: " + ex.Message;
        }

        return RedirectToAction(nameof(Projects));
    }

    // GET: /HOD/ProjectDetails/{id}
    public async Task<IActionResult> ProjectDetails(int id)
    {
        var project = await _projectService.GetProjectDetailAsync(id);
        if (project == null) return NotFound();

        return View(project);
    }

    // GET: /HOD/EditProject/{id}
    public async Task<IActionResult> EditProject(int id, string returnUrl = null)
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        ViewBag.Semesters = semesters;
        ViewBag.Majors = await _majorService.GetAllMajorsAsync();
        ViewBag.Lecturers = await _userService.GetAllUsersAsync(role: "Lecturer");

        var project = await _projectService.GetProjectDetailAsync(id);
        if (project == null) return NotFound();

        ViewBag.ReturnUrl = returnUrl;
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
    public async Task<IActionResult> UpdateProject(UpdateProjectDto dto, string returnUrl = null)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(ProjectDetails), new { id = dto.ProjectID });

        await _projectService.UpdateProjectAsync(dto);
        TempData["SuccessMessage"] = "Project updated successfully.";
        
        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(returnUrl);
            
        return RedirectToAction(nameof(Projects));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var success = await _projectService.DeleteProjectAsync(id);
        return Json(new { success, message = success ? "Xóa đề tài thành công." : "Không thể xóa đề tài này." });
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


    public async Task<IActionResult> AssignSupervisor()
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);
        
        var data = await _projectService.GetSupervisorAssignmentDataAsync(activeSemester?.SemesterID);
        ViewBag.ActiveSemester = activeSemester;
        
        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> ReassignSupervisor([FromBody] AssignSupervisorRequest request)
    {
        if (request == null || request.ProjectID <= 0 || string.IsNullOrEmpty(request.LecturerID))
            return Json(new { success = false, message = "Thông tin không hợp lệ." });

        var assignedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (success, message) = await _projectService.AssignSupervisorAsync(request.ProjectID, request.LecturerID, assignedBy);
        
        return Json(new { success, message });
    }
    public async Task<IActionResult> ReviewRounds()
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);
        
        if (activeSemester == null)
            return View(new List<ReviewRoundDto>());
 
        var rounds = await _reviewRoundService.GetReviewRoundsBySemesterAsync(activeSemester.SemesterID);
        
        // Auto-initialize 3 rounds if none exist
        if (!rounds.Any())
        {
            await _reviewRoundService.InitializeDefaultRoundsAsync(activeSemester.SemesterID);
            rounds = await _reviewRoundService.GetReviewRoundsBySemesterAsync(activeSemester.SemesterID);
        }

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

        // 2. Future Date Validation (Only for NEW rounds)
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

        // 2. Future Date Validation (Loosened for editing existing rounds)
        var now = DateTime.Now;
        // Only block if EndDate was formerly in the future and is now being moved to the past
        // Actually, just allow it for Edit.
        // if (dto.EndDate <= now) { ... }
        

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
        try 
        {
            await _reviewRoundService.DeleteReviewRoundAsync(id);
            TempData["SuccessMessage"] = "Review round deleted successfully.";
        }
        catch (System.InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(ReviewRounds));
    }

    public async Task<IActionResult> Checklists()
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);
        
        if (activeSemester == null)
        {
            ViewBag.ReviewRounds = new List<ReviewRoundDto>();
            ViewBag.Semesters = semesters;
            return View();
        }

        var rounds = await _reviewRoundService.GetReviewRoundsBySemesterAsync(activeSemester.SemesterID);
        var checklists = await _checklistService.GetBySemesterIdAsync(activeSemester.SemesterID);

        ViewBag.ReviewRounds = rounds;
        ViewBag.Checklists = checklists;
        ViewBag.ActiveSemester = activeSemester;
        ViewBag.Semesters = semesters;

        return View();
    }

    public async Task<IActionResult> ChecklistDetails(int id)
    {
        var checklist = await _checklistService.GetByRoundIdAsync(id);
        if (checklist == null) return NotFound();

        return View(checklist);
    }

    // Redirect legacy EditChecklist route → Checklists (inline edit now)
    public IActionResult EditChecklist(int id) => RedirectToAction(nameof(Checklists));

    [HttpGet]
    public async Task<IActionResult> GetChecklistJson(int id)
    {
        var checklist = await _checklistService.GetByRoundIdAsync(id);
        if (checklist == null)
        {
            var round = await _reviewRoundService.GetReviewRoundByIdAsync(id);
            if (round == null) return NotFound();
            checklist = new ChecklistDto { ReviewRoundID = id, ReviewRoundTitle = $"Round {round.RoundNumber}", Items = new List<ChecklistItemDto>() };
        }
        return Json(checklist);
    }

    [HttpPost]
    public async Task<IActionResult> CopyChecklists([FromBody] CopyChecklistRequest request)
    {
        if (request == null || request.FromSemesterId <= 0 || request.ToSemesterId <= 0)
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

        var (success, message) = await _checklistService.CopyChecklistAsync(request.FromSemesterId, request.ToSemesterId, request.RoundNumbers);
        return Json(new { success, message });
    }

    [HttpPost]
    public async Task<IActionResult> SaveChecklist([FromBody] SaveChecklistDto dto)
    {
        if (dto == null || dto.ReviewRoundID <= 0)
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

        var (success, message) = await _checklistService.SaveChecklistAsync(dto);
        return Json(new { success, message });
    }

    public IActionResult AssignReviewer() => View();
    [HttpGet("Reports")]
    public async Task<IActionResult> Reports(int? semesterId, int page = 1)
    {
        ViewData["Title"] = "Reports";
        ViewData["BreadcrumbTitle"] = "Reports";

        var semesters = await _semesterService.GetAllSemestersAsync();
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

        ViewBag.Semesters = semesters;
        ViewBag.SelectedSemesterId = semesterId == -1 ? -1 : targetSemesterId;

        var dto = await _reportService.GetHODReportAsync(targetSemesterId);
        
        var targetSemObj = targetSemesterId.HasValue ? semesters.FirstOrDefault(s => s.SemesterID == targetSemesterId.Value) : null;
        
        // Paginate workloads (pageSize = 10 to match _Pagination.cshtml)
        var workloadItems = dto.SupervisorWorkloads.Select(s => new SupervisorWorkloadItem 
        { 
            LecturerName = s.LecturerName, 
            ProjectCount = s.ProjectCount, 
            GroupCount = s.GroupCount, 
            StudentCount = s.StudentCount 
        });

        var vm = new HODReportViewModel
        {
            TotalProjects = dto.TotalProjects,
            TotalGroups = dto.TotalGroups,
            TotalStudents = dto.TotalStudents,
            TotalSupervisors = dto.TotalSupervisors,
            SemesterCode = targetSemObj?.SemesterCode ?? "Tất cả",
            AcademicYear = targetSemObj?.AcademicYear ?? "N/A",
            DraftProjects = dto.DraftProjects,
            ActiveProjects = dto.ActiveProjects,
            CompletedProjects = dto.CompletedProjects,
            CancelledProjects = dto.CancelledProjects,
            MajorDistribution = dto.MajorDistribution.Select(m => new MajorDistributionItem { MajorName = m.MajorName, ProjectCount = m.ProjectCount }).ToList(),
            RoundSubmissionStats = dto.RoundSubmissionStats.Select(r => new RoundSubmissionStat { RoundNumber = r.RoundNumber, RoundDescription = r.RoundDescription, TotalRequired = r.TotalRequired, OnTimeCount = r.OnTimeCount, LateCount = r.LateCount, NotSubmittedCount = r.NotSubmittedCount }).ToList(),
            SupervisorWorkloads = Models.PaginatedList<SupervisorWorkloadItem>.Create(workloadItems, page, 10),
            AllSupervisorWorkloads = workloadItems.ToList(),
            RoundMentorStats = dto.RoundMentorStats.Select(m => new RoundMentorDecisionStat { RoundNumber = m.RoundNumber, AcceptedCount = m.AcceptedCount, RejectedCount = m.RejectedCount, PendingCount = m.PendingCount, StoppedCount = m.StoppedCount }).ToList()
        };


        return View(vm);
    }

    
    [HttpGet]
    public async Task<IActionResult> CreateProject()
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        ViewBag.Semesters = semesters;
        ViewBag.Majors = await _majorService.GetAllMajorsAsync();
        ViewBag.Lecturers = await _userService.GetAllUsersAsync(role: "Lecturer");
        
        var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active);
        ViewBag.ActiveSemester = activeSemester;
        
        return View("EditProject", new ProjectDetailDto());
    }

    public async Task<IActionResult> DownloadSubmission(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!await _projectService.CanUserAccessSubmissionAsync(userId, id, "HeadOfDept"))
        {
            TempData["ErrorMessage"] = "You do not have permission to download this file.";
            return RedirectToAction(nameof(Index));
        }

        var fileInfo = await _projectService.GetSubmissionFileAsync(id);
        if (fileInfo == null)
        {
            TempData["ErrorMessage"] = "Unable to download the requested file.";
            return RedirectToAction(nameof(Index));
        }

        return File(fileInfo.Value.content, fileInfo.Value.contentType, fileInfo.Value.fileName);
    }

    // ============================================================
    // Session Scheduling
    // ============================================================

    public async Task<IActionResult> ScheduleSession(int roundId)
    {
        var round = await _reviewRoundService.GetReviewRoundByIdAsync(roundId);
        if (round == null) return NotFound();

        var semester = await _semesterService.GetCurrentSemesterAsync();
        if (semester == null) return NotFound();

        var allGroups = await _groupRepository.GetAllWithDetailsAsync();
        var semesterGroups = allGroups.Where(g => g.Project?.SemesterID == semester.SemesterID).ToList();
        var sessions = await _reviewRoundService.GetGroupSessionsAsync(roundId);
        var rooms = await _roomRepository.GetAllAsync();
        
        var viewModel = new ScheduleSessionViewModel
        {
            RoundId = roundId,
            RoundName = $"Vòng {round.RoundNumber}",
            Rooms = rooms.ToList(),
            Groups = semesterGroups.Select(group =>
            {
                var session = sessions.FirstOrDefault(s => s.GroupID == group.GroupID);
                return new GroupSessionListItemViewModel
                {
                    GroupId = group.GroupID,
                    GroupName = group.GroupName,
                    ProjectName = group.Project?.ProjectName ?? "N/A",
                    SessionId = session?.SessionID,
                    ScheduledAt = session?.ScheduledAt,
                    RoomId = session?.RoomID,
                    MeetLink = session?.MeetLink,
                    Notes = session?.Notes
                };
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SaveSession([FromBody] ScheduleSessionUpdateDto dto)
    {
        if (dto == null) return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        
        var success = await _reviewRoundService.ScheduleSessionAsync(dto);
        return Json(new { success, message = success ? "Lưu lịch họp thành công." : "Có lỗi xảy ra." });
    }

    [HttpPost]
    public async Task<IActionResult> GenerateSessionMeetLink(int sessionId)
    {
        var success = await _reviewRoundService.GenerateMeetingLinkAsync(sessionId);
        return Json(new { success, message = success ? "Đã tạo link Google Meet thành công." : "Không thể tạo link Meet." });
    }
}
