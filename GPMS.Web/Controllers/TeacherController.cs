using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Web.ViewModels;
using GPMS.Domain.Enums;
using GPMS.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "Lecturer")]
public class TeacherController : Controller
{
    private readonly IReviewerAssignmentRepository _reviewerRepo;
    private readonly IReviewRoundRepository _roundRepo;
    private readonly IEvaluationRepository _evaluationRepo;
    private readonly IProjectGroupRepository _groupRepo;
    private readonly IFeedbackRepository _feedbackRepo;

    public TeacherController(
        IReviewerAssignmentRepository reviewerRepo,
        IReviewRoundRepository roundRepo,
        IEvaluationRepository evaluationRepo,
        IProjectGroupRepository groupRepo,
        IFeedbackRepository feedbackRepo)
    {
        _reviewerRepo = reviewerRepo;
        _roundRepo = roundRepo;
        _evaluationRepo = evaluationRepo;
        _groupRepo = groupRepo;
        _feedbackRepo = feedbackRepo;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> AssignedGroups()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Auth");

        var assignments = await _reviewerRepo.GetByReviewerAsync(userId);
        var viewModels = assignments.Select(a =>
        {
            var sessionInfo = a.Group?.ReviewSessions?.FirstOrDefault(rs => rs.ReviewRoundID == a.ReviewRoundID);
            
            var vm = new AssignedGroupViewModel
            {
                ReviewRoundID = a.ReviewRoundID,
                GroupID = a.GroupID,
                ProjectName = a.Group?.Project?.ProjectName ?? "",
                GroupName = a.Group?.GroupName ?? "",
                RoundType = a.ReviewRound?.RoundType ?? RoundType.Online,
                RoundNumber = a.ReviewRound?.RoundNumber ?? 0,
                StartDate = a.ReviewRound?.StartDate ?? System.DateTime.MinValue,
                ScheduledAt = sessionInfo?.ScheduledAt
            };

            if (sessionInfo != null)
            {
                if (a.ReviewRound?.RoundType == RoundType.Online)
                {
                    vm.MeetLink = sessionInfo.MeetLink;
                }
                else if (sessionInfo.Room != null)
                {
                    vm.RoomCode = sessionInfo.Room.RoomCode;
                }
            }
            
            return vm;
        }).ToList();

        var dashboardVm = new ReviewerDashboardViewModel
        {
            AssignedGroups = viewModels
        };

        return View(dashboardVm);
    }

    [HttpGet]
    public async Task<IActionResult> EvaluationForm(int roundId, int groupId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth");

        var round = await _roundRepo.GetByIdWithChecklistAsync(roundId);
        if (round == null || round.ReviewChecklist == null) return NotFound("Checklist not found for this round.");

        var group = await _groupRepo.GetByIdAsync(groupId);
        if (group == null) return NotFound("Group not found.");

        var existingEvaluation = await _evaluationRepo.GetByReviewerAndGroupAsync(roundId, userId, groupId);

        var vm = new EvaluationFormViewModel
        {
            EvaluationID = existingEvaluation?.EvaluationID ?? 0,
            ReviewRoundID = roundId,
            GroupID = groupId,
            ProjectName = "Project",
            GroupName = group.GroupName,
            ChecklistTitle = round.ReviewChecklist.Title,
            Status = existingEvaluation?.Status ?? EvaluationStatus.Draft,
            GeneralComment = existingEvaluation?.Feedback?.Content
        };

        foreach (var item in round.ReviewChecklist.ChecklistItems.OrderBy(i => i.OrderIndex))
        {
            var detail = existingEvaluation?.EvaluationDetails?.FirstOrDefault(d => d.ItemID == item.ItemID);
            vm.Items.Add(new EvaluationItemViewModel
            {
                ItemID = item.ItemID,
                ItemCode = item.ItemCode,
                ItemContent = item.ItemContent,
                MaxScore = item.MaxScore,
                Weight = item.Weight,
                Score = detail?.Score,
                Comment = detail?.Comment
            });
        }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> SaveDraft([FromBody] EvaluationFormViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var evaluation = await _evaluationRepo.GetByReviewerAndGroupAsync(model.ReviewRoundID, userId, model.GroupID);
        if (evaluation == null)
        {
            evaluation = new Evaluation
            {
                ReviewRoundID = model.ReviewRoundID,
                ReviewerID = userId,
                GroupID = model.GroupID,
                Status = EvaluationStatus.Draft,
                TotalScore = null
            };
            await _evaluationRepo.AddAsync(evaluation);
        }
        else if (evaluation.Status == EvaluationStatus.Submitted)
        {
            return BadRequest("Cannot modify a submitted evaluation.");
        }

        foreach (var item in model.Items)
        {
            var detail = evaluation.EvaluationDetails.FirstOrDefault(d => d.ItemID == item.ItemID);
            if (detail == null)
            {
                if (item.Score.HasValue || !string.IsNullOrEmpty(item.Comment))
                {
                    evaluation.EvaluationDetails.Add(new EvaluationDetail
                    {
                        ItemID = item.ItemID,
                        Score = item.Score ?? 0,
                        Comment = item.Comment
                    });
                }
            }
            else
            {
                detail.Score = item.Score ?? detail.Score;
                detail.Comment = item.Comment;
            }
        }

        await _evaluationRepo.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitEvaluation([FromBody] EvaluationFormViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var evaluation = await _evaluationRepo.GetByReviewerAndGroupAsync(model.ReviewRoundID, userId, model.GroupID);
        if (evaluation == null)
        {
            evaluation = new Evaluation
            {
                ReviewRoundID = model.ReviewRoundID,
                ReviewerID = userId,
                GroupID = model.GroupID,
                Status = EvaluationStatus.Submitted,
                SubmittedAt = System.DateTime.UtcNow
            };
            await _evaluationRepo.AddAsync(evaluation);
        }
        else if (evaluation.Status == EvaluationStatus.Submitted)
        {
            return BadRequest("Evaluation already submitted.");
        }
        else
        {
            evaluation.Status = EvaluationStatus.Submitted;
            evaluation.SubmittedAt = System.DateTime.UtcNow;
        }

        decimal totalScore = 0;
        foreach (var item in model.Items)
        {
            if (!item.Score.HasValue) return BadRequest($"Missing score for item {item.ItemCode}");

            var detail = evaluation.EvaluationDetails.FirstOrDefault(d => d.ItemID == item.ItemID);
            if (detail == null)
            {
                evaluation.EvaluationDetails.Add(new EvaluationDetail
                {
                    ItemID = item.ItemID,
                    Score = item.Score.Value,
                    Comment = item.Comment
                });
            }
            else
            {
                detail.Score = item.Score.Value;
                detail.Comment = item.Comment;
            }
            totalScore += item.Score.Value * item.Weight;
        }
        evaluation.TotalScore = totalScore;

        if (evaluation.Feedback == null)
        {
            evaluation.Feedback = new Feedback
            {
                Content = model.GeneralComment ?? string.Empty,
                FeedbackApproval = new FeedbackApproval
                {
                    ApprovalStatus = ApprovalStatus.Pending
                }
            };
        }
        else
        {
            evaluation.Feedback.Content = model.GeneralComment ?? string.Empty;
        }

        await _evaluationRepo.SaveChangesAsync();
        return Ok(new { success = true, redirectUrl = Url.Action("AssignedGroups") });
    }

    [HttpGet]
    public async Task<IActionResult> FeedbackApprovals()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth");

        var feedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(userId);
        var vm = new SupervisorFeedbackReviewViewModel
        {
            PendingFeedbacks = feedbacks.Select(f => new PendingFeedbackViewModel
            {
                FeedbackID = f.FeedbackID,
                ProjectName = f.Evaluation?.Group?.Project?.ProjectName ?? "N/A",
                GroupName = f.Evaluation?.Group?.GroupName ?? "N/A",
                ReviewerName = f.Evaluation?.Reviewer?.FullName ?? "Unknown",
                RoundName = $"Round {f.Evaluation?.ReviewRound?.RoundNumber}",
                Score = f.Evaluation?.TotalScore,
                Content = f.Content,
                SubmittedAt = f.Evaluation?.SubmittedAt ?? f.CreatedAt
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessFeedbackApproval([FromBody] ApproveFeedbackRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var feedback = await _feedbackRepo.GetByIdWithDetailsAsync(request.FeedbackID);
        if (feedback == null || feedback.FeedbackApproval == null) return NotFound("Feedback not found.");

        feedback.FeedbackApproval.ApprovalStatus = request.Status;
        feedback.FeedbackApproval.SupervisorComment = request.SupervisorComment ?? string.Empty;
        feedback.FeedbackApproval.SupervisorID = userId;
        feedback.FeedbackApproval.ApprovedAt = System.DateTime.UtcNow;

        if (request.Status == ApprovalStatus.Approved)
        {
            feedback.FeedbackApproval.IsVisibleToStudent = false; 
        }

        await _feedbackRepo.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> ReleaseFeedback([FromBody] int feedbackId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var feedback = await _feedbackRepo.GetByIdWithDetailsAsync(feedbackId);
        if (feedback == null || feedback.FeedbackApproval == null || feedback.FeedbackApproval.SupervisorID != userId) 
            return NotFound("Feedback not found or you are not authorized.");

        if (feedback.FeedbackApproval.ApprovalStatus != ApprovalStatus.Approved)
            return BadRequest("Only approved feedback can be released.");

        feedback.FeedbackApproval.IsVisibleToStudent = true;
        await _feedbackRepo.SaveChangesAsync();

        return Ok(new { success = true });
    }
}
