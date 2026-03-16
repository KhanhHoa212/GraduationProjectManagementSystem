using ClosedXML.Excel;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Services;
using GPMS.Web.Models;
using GPMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GPMS.Web.Helpers;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "Admin,HeadOfDept")]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ISemesterService _semesterService;
    private readonly IProjectService _projectService;
    private readonly IMemoryCache _cache;

    private const string LogCacheKey = "admin_system_logs";
    private const string VisitKeyPrefix = "visits:";

    public AdminController(
        IUserService userService,
        ISemesterService semesterService,
        IProjectService projectService,
        IMemoryCache cache)
    {
        _userService = userService;
        _semesterService = semesterService;
        _projectService = projectService;
        _cache = cache;
    }

    // ── Helper: record a visit for today ──────────────────────────────────
    private async Task RecordVisit()
    {
        var dateKey = DateTime.Now.ToString("yyyy-MM-dd");
        var cacheKey = VisitKeyPrefix + dateKey;
        
        // 1. Get current count from cache or file
        var count = _cache.Get<int?>(cacheKey);
        if (count == null)
        {
            var savedVisits = await FileStorageHelper.LoadAsync<Dictionary<string, int>>("visits.json") ?? new();
            count = savedVisits.GetValueOrDefault(dateKey, 0);
        }

        var newCount = count.Value + 1;

        // 2. Update Cache
        _cache.Set(cacheKey, newCount, TimeSpan.FromDays(8));

        // 3. Persist all visits to file
        var allVisits = await FileStorageHelper.LoadAsync<Dictionary<string, int>>("visits.json") ?? new();
        allVisits[dateKey] = newCount;
        await FileStorageHelper.SaveAsync("visits.json", allVisits);
    }

    // ── Helper: append to system log ──────────────────────────────────────
    private async Task AppendLog(string action, string target, string status = "Success")
    {
        // 1. Load from cache or file
        var logs = _cache.Get<List<SystemLogEntry>>(LogCacheKey);
        if (logs == null)
        {
            logs = await FileStorageHelper.LoadAsync<List<SystemLogEntry>>("logs.json") ?? new();
        }

        // 2. Add new entry
        logs.Insert(0, new SystemLogEntry
        {
            Action = action,
            Target = target,
            Timestamp = DateTime.Now,
            Status = status
        });

        // 3. Keep latest 50
        if (logs.Count > 50) logs = logs.Take(50).ToList();

        // 4. Update Cache and File
        _cache.Set(LogCacheKey, logs, TimeSpan.FromDays(30));
        await FileStorageHelper.SaveAsync("logs.json", logs);
    }

    // ── DASHBOARD ─────────────────────────────────────────────────────────

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Dashboard()
    {
        await RecordVisit();

        var (total, students, lecturers, admins, hods) = await _userService.GetUserCountsAsync();

        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        int projectCount = 0;
        if (currentSemester != null)
        {
            var projects = await _projectService.GetProjectsBySemesterAsync(currentSemester.SemesterID);
            projectCount = projects.Count();
        }

        // 1. Logs: Load from cache or file
        var logs = _cache.Get<List<SystemLogEntry>>(LogCacheKey);
        if (logs == null)
        {
            logs = await FileStorageHelper.LoadAsync<List<SystemLogEntry>>("logs.json") ?? new();
            _cache.Set(LogCacheKey, logs, TimeSpan.FromDays(30));
        }

        // 2. Visits: Load from cache or file
        var savedVisits = await FileStorageHelper.LoadAsync<Dictionary<string, int>>("visits.json") ?? new();
        var visits = new List<DailyVisit>();
        for (int i = 6; i >= 0; i--)
        {
            var day = DateTime.Now.AddDays(-i);
            var dateKey = day.ToString("yyyy-MM-dd");
            var cacheKey = VisitKeyPrefix + dateKey;
            
            if (!_cache.TryGetValue(cacheKey, out int count))
            {
                count = savedVisits.GetValueOrDefault(dateKey, 0);
                _cache.Set(cacheKey, count, TimeSpan.FromDays(8));
            }

            visits.Add(new DailyVisit
            {
                DayLabel = day.ToString("dd/MM"),
                Count = count
            });
        }

        var vm = new DashboardViewModel
        {
            TotalUsers = total,
            TotalStudents = students,
            TotalLecturers = lecturers,
            TotalAdmins = admins,
            TotalHODs = hods,
            CurrentSemesterCode = currentSemester?.SemesterCode ?? "N/A",
            CurrentSemesterProjectCount = projectCount,
            RecentLogs = logs.Take(10).ToList(),
            WeeklyVisits = visits
        };

        return View(vm);
    }

    // ── EXPORT EXCEL ──────────────────────────────────────────────────────

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportReport()
    {
        var (total, students, lecturers, admins, hods) = await _userService.GetUserCountsAsync();

        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        int projectCount = 0;
        if (currentSemester != null)
        {
            var projects = await _projectService.GetProjectsBySemesterAsync(currentSemester.SemesterID);
            projectCount = projects.Count();
        }

        // Fetch real visits from file for accuracy
        var savedVisits = await FileStorageHelper.LoadAsync<Dictionary<string, int>>("visits.json") ?? new();
        var todayKey = DateTime.Now.ToString("yyyy-MM-dd");
        var todayVisits = _cache.Get<int?>(VisitKeyPrefix + todayKey) ?? savedVisits.GetValueOrDefault(todayKey, 0);
        int totalVisitsCount = savedVisits.Values.Sum();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Dashboard Report");

        // Title
        ws.Cell(1, 1).Value = "GPMS – Admin Dashboard Report";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 2).Merge();

        ws.Cell(2, 1).Value = "Generated at:";
        ws.Cell(2, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        ws.Cell(3, 1).Value = ""; // spacer

        // Headers
        ws.Cell(4, 1).Value = "Metric";
        ws.Cell(4, 2).Value = "Value";
        ws.Row(4).Style.Font.Bold = true;
        ws.Row(4).Style.Fill.BackgroundColor = XLColor.FromHtml("#F27123");
        ws.Row(4).Style.Font.FontColor = XLColor.White;

        int row = 5;
        void AddRow(string metric, object value)
        {
            ws.Cell(row, 1).Value = metric;
            ws.Cell(row, 2).Value = value?.ToString() ?? "";
            row++;
        }

        AddRow("Total Users", total);
        AddRow("Total Students", students);
        AddRow("Total Lecturers", lecturers);
        AddRow("Total Admins", admins);
        AddRow("Total HODs", hods);
        AddRow("Current Semester", currentSemester?.SemesterCode ?? "N/A");
        AddRow("Projects in Current Semester", projectCount);
        AddRow("Today's Visits", todayVisits);
        AddRow("Total Visits (all time recorded)", totalVisitsCount);

        ws.Column(1).Width = 35;
        ws.Column(2).Width = 20;
        ws.Columns().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // Add border to data rows
        ws.Range(4, 1, row - 1, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(4, 1, row - 1, 2).Style.Border.InsideBorder  = XLBorderStyleValues.Thin;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var fileName = $"GPMS_Dashboard_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    // ── USER MANAGEMENT ───────────────────────────────────────────────────

    public async Task<IActionResult> Index(string? search, string? role, GPMS.Domain.Enums.UserStatus? status, int page = 1)
    {
        var users = await _userService.GetAllUsersAsync(search, role, status);

        ViewData["CurrentSearch"] = search;
        ViewData["CurrentRole"] = role;
        ViewData["CurrentStatus"] = status;

        var viewModels = users.Select(u => new UserViewModel
        {
            UserID = u.UserID,
            Username = u.Username ?? string.Empty,
            Email = u.Email ?? string.Empty,
            FullName = u.FullName,
            Phone = u.Phone,
            Status = u.Status,
            Roles = u.Roles
        }).ToList();

        // Apply pagination (default 10 per page)
        var paginatedList = PaginatedList<UserViewModel>.Create(viewModels, page, 10);

        return View(paginatedList);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EditUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EditUserViewModel model)
    {
        if (model.UserID != null) model.UserID = model.UserID.ToUpper();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Role == "Admin")
        {
            ModelState.AddModelError("Role", "Cannot create Admin role.");
            return View(model);
        }

        var dto = new CreateUserDto
        {
            UserID = model.UserID,
            Username = model.Email ?? string.Empty,
            Email = model.Email ?? string.Empty,
            FullName = model.FullName,
            Phone = model.Phone,
            Role = model.Role,
            Status = model.Status
        };

        try
        {
            await _userService.CreateUserAsync(dto);
            await AppendLog("User Created", $"{model.FullName} ({model.UserID})");
            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (System.InvalidOperationException ex)
        {
            if (ex.Message.Contains("User ID"))
                ModelState.AddModelError("UserID", ex.Message);
            else if (ex.Message.Contains("Email"))
                ModelState.AddModelError("Email", ex.Message);
            else if (ex.Message.Contains("Username"))
                ModelState.AddModelError("Username", ex.Message);
            else
                ModelState.AddModelError("", ex.Message);

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        var role = user.Roles.Contains("Admin") ? "Admin"
                 : user.Roles.Contains("HeadOfDept") ? "HeadOfDept"
                 : user.Roles.Contains("Lecturer") ? "Lecturer"
                 : "Student";

        var model = new EditUserViewModel
        {
            UserID = user.UserID,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Status = user.Status,
            Role = role
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (model.UserID != null) model.UserID = model.UserID.ToUpper();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (string.IsNullOrEmpty(model.UserID)) return BadRequest("User ID is required");
        var user = await _userService.GetUserByIdAsync(model.UserID);
        if (user == null) return NotFound();

        bool wasAdmin = user.Roles.Contains("Admin");
        if (wasAdmin && model.Role != "Admin")
        {
            TempData["ErrorMessage"] = "Cannot change Admin role.";
            return RedirectToAction(nameof(Index));
        }

        var dto = new UpdateUserDto
        {
            UserID = model.UserID,
            Username = model.Email ?? string.Empty,
            Email = model.Email ?? string.Empty,
            FullName = model.FullName,
            Phone = model.Phone,
            Role = model.Role,
            Status = model.Status
        };

        try
        {
            await _userService.UpdateUserAsync(dto);
            await AppendLog("User Updated", $"{model.UserID} – {model.FullName}");
            TempData["SuccessMessage"] = "Update user successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (System.InvalidOperationException ex)
        {
            if (ex.Message.Contains("Email"))
                ModelState.AddModelError("Email", ex.Message);
            else if (ex.Message.Contains("Username"))
                ModelState.AddModelError("Username", ex.Message);
            else
                ModelState.AddModelError("", ex.Message);

            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        await _userService.ToggleUserStatusAsync(id);
        TempData["SuccessMessage"] = "Changed user status successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ── SEMESTER MANAGEMENT ───────────────────────────────────────────────

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SemesterIndex()
    {
        var semesters = await _semesterService.GetAllSemestersAsync();
        var viewModels = semesters.Select(s => new SemesterViewModel
        {
            SemesterID = s.SemesterID,
            SemesterCode = s.SemesterCode,
            AcademicYear = s.AcademicYear,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status,
            ProjectsCount = s.ProjectsCount
        }).ToList();

        return View("SemesterIndex", viewModels);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult SemesterCreate()
    {
        return View("SemesterCreate", new EditSemesterViewModel());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SemesterCreate(EditSemesterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new CreateSemesterDto
        {
            SemesterCode = model.SemesterCode,
            AcademicYear = model.AcademicYear,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Status = model.Status
        };

        var error = await _semesterService.CreateSemesterAsync(dto);

        if (error != null)
        {
            ModelState.AddModelError("", error);
            return View(model);
        }
        
        await AppendLog("Semester Created", model.SemesterCode);
        TempData["SuccessMessage"] = "Semester created successfully.";
        return RedirectToAction(nameof(SemesterIndex));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> SemesterEdit(int id)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(id);
        if (semester == null) return NotFound();

        var model = new EditSemesterViewModel
        {
            SemesterID = semester.SemesterID,
            SemesterCode = semester.SemesterCode,
            AcademicYear = semester.AcademicYear,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            Status = semester.Status
        };

        return View("SemesterEdit", model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SemesterEdit(EditSemesterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new UpdateSemesterDto
        {
            SemesterID = model.SemesterID,
            SemesterCode = model.SemesterCode,
            AcademicYear = model.AcademicYear,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Status = model.Status
        };

        var error = await _semesterService.UpdateSemesterAsync(dto);

        if (error != null)
        {
            ModelState.AddModelError("", error);
            return View(model);
        }
        
        await AppendLog("Semester Updated", model.SemesterCode);
        TempData["SuccessMessage"] = "Semester updated successfully.";
        return RedirectToAction(nameof(SemesterIndex));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SemesterDelete(int id)
    {
        bool result = await _semesterService.DeleteSemesterAsync(id);
        if (!result)
        {
            TempData["ErrorMessage"] = "Cannot delete semester because it has projects or was not found.";
        }
        else
        {
            TempData["SuccessMessage"] = "Deleted semester successfully!";
        }
        return RedirectToAction(nameof(SemesterIndex));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SystemLogs()
    {
        var logs = _cache.Get<List<SystemLogEntry>>(LogCacheKey);
        if (logs == null)
        {
            logs = await FileStorageHelper.LoadAsync<List<SystemLogEntry>>("logs.json") ?? new();
            _cache.Set(LogCacheKey, logs, TimeSpan.FromDays(30));
        }
        return View(logs);
    }
}
