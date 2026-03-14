using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly IFeedbackRepository _feedbackRepo;
    private readonly IProjectService _projectService;

    public StudentController(IFeedbackRepository feedbackRepo, IProjectService projectService)
    {
        _feedbackRepo = feedbackRepo;
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

    public async Task<IActionResult> FeedbackHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth");

        var feedbacks = await _feedbackRepo.GetVisibleFeedbacksForStudentAsync(userId);
        
        var vm = new StudentFeedbackHistoryViewModel
        {
            Feedbacks = feedbacks.Select(f => new StudentFeedbackDetailsViewModel
            {
                FeedbackID = f.FeedbackID,
                ProjectName = f.Evaluation.Group.Project?.ProjectName ?? "N/A",
                RoundName = $"Round {f.Evaluation.ReviewRound.RoundNumber}",
                Score = f.Evaluation.TotalScore,
                Content = f.Content,
                SubmittedAt = f.Evaluation.SubmittedAt ?? f.CreatedAt,
                ReviewerName = "Hidden Reviewer",
                Items = f.Evaluation.EvaluationDetails.Select(d => new EvaluationItemViewModel
                {
                    ItemID = d.ItemID,
                    ItemCode = d.Item.ItemCode,
                    ItemContent = d.Item.ItemContent,
                    MaxScore = d.Item.MaxScore,
                    Weight = d.Item.Weight,
                    Score = d.Score,
                    Comment = d.Comment
                }).ToList()
            }).ToList()
        };

        return View(vm);
    }
}
