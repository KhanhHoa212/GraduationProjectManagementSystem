using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Services;
using GPMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "Admin,HeadOfDept")]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ISemesterService _semesterService;

    public AdminController(IUserService userService, ISemesterService semesterService)
    {
        _userService = userService;
        _semesterService = semesterService;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Dashboard()
    {
        return View();
    }

    // --- USER MANAGEMENT ---

    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllUsersAsync();
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

        return View(viewModels);
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
            Username = model.Username ?? string.Empty,
            Email = model.Email ?? string.Empty,
            FullName = model.FullName,
            Phone = model.Phone,
            Role = model.Role,
            Status = model.Status
        };

        await _userService.CreateUserAsync(dto);
        TempData["SuccessMessage"] = "Created user successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        var role = user.Roles.Contains("Admin") ? "Admin" 
                 : user.Roles.Contains("Lecturer") ? "Lecturer" 
                 : "Student";

        var model = new EditUserViewModel
        {
            UserID = user.UserID,
            Username = user.Username,
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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

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
            Username = model.Username ?? string.Empty,
            Email = model.Email ?? string.Empty,
            FullName = model.FullName,
            Phone = model.Phone,
            Role = model.Role,
            Status = model.Status
        };

        await _userService.UpdateUserAsync(dto);
        TempData["SuccessMessage"] = "Update user successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        await _userService.ToggleUserStatusAsync(id);
        TempData["SuccessMessage"] = "Changed user status successfully!";
        return RedirectToAction(nameof(Index));
    }

    // --- SEMESTER MANAGEMENT ---

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
        {
            return View("SemesterCreate", model);
        }

        var dto = new CreateSemesterDto
        {
            SemesterCode = model.SemesterCode,
            AcademicYear = model.AcademicYear,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Status = model.Status
        };

        await _semesterService.CreateSemesterAsync(dto);
        TempData["SuccessMessage"] = "Created semester successfully!";
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
        {
            return View("SemesterEdit", model);
        }

        var dto = new UpdateSemesterDto
        {
            SemesterID = model.SemesterID,
            SemesterCode = model.SemesterCode,
            AcademicYear = model.AcademicYear,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Status = model.Status
        };

        await _semesterService.UpdateSemesterAsync(dto);
        TempData["SuccessMessage"] = "Updated semester successfully!";
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
}
