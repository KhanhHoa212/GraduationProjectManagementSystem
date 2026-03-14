using GPMS.Application.Interfaces.Services;
using GPMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly IProjectService _projectService;

    public StudentController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public async Task<IActionResult> Index()
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Login", "Auth");

        var project = await _projectService.GetProjectByStudentAsync(studentId);
        var submissions = await _projectService.GetDashboardSubmissionsAsync(studentId);
        var feedbacks = await _projectService.GetDashboardFeedbacksAsync(studentId, 5);

        var viewModel = new StudentDashboardViewModel
        {
            Project = project,
            ActiveSubmissions = submissions,
            RecentFeedbacks = feedbacks
        };

        return View(viewModel);
    }
}
