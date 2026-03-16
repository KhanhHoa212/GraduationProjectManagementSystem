using GPMS.Application.Interfaces.Services;
using GPMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IReviewRoundService _reviewRoundService;

    public StudentController(IProjectService projectService, IReviewRoundService reviewRoundService)
    {
        _projectService = projectService;
        _reviewRoundService = reviewRoundService;
    }

    public async Task<IActionResult> Index()
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Login", "Auth");

        var project = await _projectService.GetProjectByStudentAsync(studentId);
        var submissions = await _projectService.GetDashboardSubmissionsAsync(studentId);
        var feedbacks = await _projectService.GetDashboardFeedbacksAsync(studentId, 5);

        IEnumerable<GPMS.Application.DTOs.ReviewRoundDto> reviewRounds = new List<GPMS.Application.DTOs.ReviewRoundDto>();
        if (project != null)
        {
            reviewRounds = await _reviewRoundService.GetReviewRoundsBySemesterAsync(project.SemesterID);
        }

        var viewModel = new StudentDashboardViewModel
        {
            Project = project,
            ActiveSubmissions = submissions,
            RecentFeedbacks = feedbacks,
            ReviewRounds = reviewRounds
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Submissions(int? round = null)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Login", "Auth");

        var project = await _projectService.GetProjectByStudentAsync(studentId);
        var submissions = await _projectService.GetSubmissionsByStudentAsync(studentId);

        IEnumerable<GPMS.Application.DTOs.ReviewRoundDto> reviewRounds = new List<GPMS.Application.DTOs.ReviewRoundDto>();
        int activeRound = 0;

        if (project != null)
        {
            var rounds = await _reviewRoundService.GetReviewRoundsBySemesterAsync(project.SemesterID);
            reviewRounds = rounds;
            
            if (round.HasValue && rounds.Any(r => r.RoundNumber == round.Value))
            {
                activeRound = round.Value;
            }
            else
            {
                var current = rounds.FirstOrDefault(r => r.Status == GPMS.Domain.Enums.RoundStatus.Ongoing)
                           ?? rounds.FirstOrDefault(r => r.Status == GPMS.Domain.Enums.RoundStatus.Planned)
                           ?? rounds.LastOrDefault();
                
                activeRound = current?.RoundNumber ?? 0;
            }
        }

        var viewModel = new SubmissionsViewModel
        {
            Project = project,
            ReviewRounds = reviewRounds,
            Submissions = submissions,
            ActiveRoundNumber = activeRound
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitWork(int requirementId, IFormFile file)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Login", "Auth");

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file to upload.";
            return RedirectToAction("Submissions");
        }

        var (success, message) = await _projectService.SubmitProjectWorkAsync(studentId, requirementId, file);

        if (success)
        {
            TempData["Success"] = message;
        }
        else
        {
            TempData["Error"] = message;
        }

        return RedirectToAction("Submissions");
    }

    public async Task<IActionResult> Feedback(int? roundId)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Login", "Auth");

        var feedbackData = await _projectService.GetStudentFeedbackAsync(studentId, roundId);
        var viewModel = new StudentFeedbackViewModel { Feedback = feedbackData };

        return View(viewModel);
    }
}
