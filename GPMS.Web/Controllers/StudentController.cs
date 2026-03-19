using GPMS.Application.DTOs;
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
    private readonly INotificationService _notificationService;

    public StudentController(IProjectService projectService, IReviewRoundService reviewRoundService, INotificationService notificationService)
    {
        _projectService = projectService;
        _reviewRoundService = reviewRoundService;
        _notificationService = notificationService;
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

    public async Task<IActionResult> Schedule()
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Login", "Auth");

        var schedule = await _projectService.GetProjectDefenseScheduleAsync(studentId);
        return View(schedule);
    }

    public async Task<IActionResult> Notifications(string type = "All")
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Login", "Auth");

        var notifications = await _notificationService.GetNotificationsByRecipientAsync(studentId);
        var unreadCount = await _notificationService.GetUnreadCountAsync(studentId);

        var viewModel = new NotificationsViewModel
        {
            AllNotifications = notifications,
            DeadlineNotifications = notifications.Where(n => n.Type == GPMS.Domain.Enums.NotificationType.Deadline),
            FeedbackNotifications = notifications.Where(n => n.Type == GPMS.Domain.Enums.NotificationType.Feedback),
            UnreadCount = unreadCount,
            ActiveTab = type
        };

        IEnumerable<NotificationDto> filteredNotifications = type switch
        {
            "Unread" => viewModel.AllNotifications.Where(n => !n.IsRead),
            "Deadline" => viewModel.DeadlineNotifications,
            "Feedback" => viewModel.FeedbackNotifications,
            _ => viewModel.AllNotifications
        };

        viewModel.GroupedNotifications = filteredNotifications
            .GroupBy(n => GetDateGroupName(n.CreatedAt))
            .ToDictionary(g => g.Key, g => g.ToList());

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Login", "Auth");

        await _notificationService.MarkAllAsReadAsync(studentId);
        return RedirectToAction("Notifications");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleReadStatus(int id, string type)
    {
        await _notificationService.ToggleReadStatusAsync(id);
        return RedirectToAction("Notifications", new { type });
    }

    private string GetDateGroupName(DateTime date)
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        if (date.Date == today) return "TODAY";
        if (date.Date == yesterday) return "YESTERDAY";
        return date.ToString("dd/MM/yyyy");
    }
}
