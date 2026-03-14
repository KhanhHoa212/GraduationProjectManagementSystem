using GPMS.Application.DTOs.Lecturer;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Enums;
using GPMS.Web.ViewModels.Lecturer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GPMS.Web.Controllers;

[Authorize]
public class LecturerController : Controller
{
    private readonly ILecturerService _lecturerService;

    public LecturerController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "GV001";

    // -------------------------------------------------------
    // Dashboard
    // -------------------------------------------------------
    public async Task<IActionResult> Dashboard()
    {
        var dto = await _lecturerService.GetDashboardDataAsync(GetUserId());
        var vm = new LecturerDashboardViewModel
        {
            GroupsMentoring = dto.MentoringGroupsCount,
            PendingApprovals = dto.PendingApprovalsCount,
            AssignedReviews = dto.AssignedReviewsCount,
            UpcomingDeadlines = dto.UpcomingDeadlinesCount,
            RecentActivities = dto.RecentActivities.Select(a => new RecentActivityItem
            {
                Title = a.Title,
                Description = a.Description,
                Timestamp = a.Timestamp,
                IconName = a.Icon,
                IconColor = a.IconBgColor,
                ActionUrl = a.ActionUrl,
                ActionLabel = a.ActionText
            }).ToList(),
            TodaySchedule = dto.TodaysSchedule.Select(s => new ScheduleItem
            {
                Title = s.Title,
                Location = s.Location,
                ScheduledAt = s.StartTime,
                IsActive = s.IsHighlight
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Projects
    // -------------------------------------------------------
    public async Task<IActionResult> Projects()
    {
        var dto = await _lecturerService.GetMentoredProjectsAsync(GetUserId());
        var vm = new MentoredProjectsViewModel
        {
            Groups = dto.Projects.Select(p => new MentoredGroupRow
            {
                GroupID = p.GroupId,
                GroupName = p.GroupName,
                ProjectName = p.ProjectName,
                MemberNames = p.MemberNames,
                CurrentRound = 1,
                RoundType = p.CurrentRound,
                Status = p.Status,
                ProgressPercent = p.ProgressPercent
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Project Group Detail
    // -------------------------------------------------------
    public async Task<IActionResult> ProjectGroupDetail(int id = 100)
    {
        var dto = await _lecturerService.GetProjectGroupDetailAsync(id);
        var vm = new ProjectGroupDetailViewModel
        {
            GroupID = dto.GroupId,
            GroupName = dto.GroupName,
            ProjectName = dto.ProjectName,
            Members = dto.Members.Select(m => new GroupMemberItem
            {
                UserId = m.UserId,
                FullName = m.FullName,
                Role = m.RoleInGroup
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Feedback Approvals
    // -------------------------------------------------------
    public async Task<IActionResult> FeedbackApprovals()
    {
        var dto = await _lecturerService.GetPendingApprovalsAsync(GetUserId());
        var vm = new FeedbackApprovalsViewModel
        {
            Approvals = dto.PendingFeedbacks.Select(f => new FeedbackApprovalRow
            {
                FeedbackID = f.FeedbackId,
                EvaluationID = f.EvaluationId,
                GroupName = f.GroupName,
                ReviewerName = f.ReviewerName,
                SubmittedAt = f.SubmittedAt,
                ApprovalStatus = ApprovalStatus.Pending
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Feedback Approval Detail
    // -------------------------------------------------------
    public async Task<IActionResult> FeedbackApprovalDetail(int id = 1)
    {
        var dto = await _lecturerService.GetFeedbackApprovalDetailAsync(id);
        var vm = new FeedbackApprovalDetailViewModel
        {
            FeedbackID = dto.FeedbackId,
            EvaluationID = dto.EvaluationId,
            GroupName = dto.GroupName,
            GroupID = dto.GroupId,
            ReviewerName = dto.ReviewerName,
            RoundNumber = dto.CurrentRoundIndex,
            TotalScore = dto.TotalScore,
            FeedbackContent = dto.FeedbackContent,
            GroupMembers = dto.Members.Select(m => new GroupMemberItem
            {
                UserId = m.UserId,
                FullName = m.FullName,
                Role = m.RoleInGroup
            }).ToList(),
            EvaluationDetails = dto.Scores.Select(s => new EvalDetailRow
            {
                ItemContent = s.CriteriaName,
                MaxScore = s.MaxScore,
                Weight = s.WeightPercentage / 100m,
                Score = s.Score
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Review Assignments
    // -------------------------------------------------------
    public async Task<IActionResult> ReviewAssignments()
    {
        var dto = await _lecturerService.GetReviewAssignmentsAsync(GetUserId());
        var vm = new ReviewAssignmentsViewModel
        {
            Assignments = dto.Assignments.Select(a => new ReviewAssignmentRow
            {
                AssignmentID = a.AssignmentId,
                GroupID = a.GroupId,
                GroupName = a.GroupName,
                ProjectName = a.ProjectName,
                RoundNumber = a.RoundNumber,
                RoundType = a.RoundType,
                ScheduledAt = a.ScheduledAt,
                Location = a.Location,
                HasEvaluation = a.HasEvaluation,
                EvaluationID = a.EvaluationId
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Evaluation Form
    // -------------------------------------------------------
    public async Task<IActionResult> EvaluationForm(int id = 1)
    {
        var dto = await _lecturerService.GetEvaluationFormAsync(id);
        var vm = new EvaluationFormViewModel
        {
            AssignmentID = dto.AssignmentId,
            GroupID = dto.GroupId,
            GroupName = dto.GroupName,
            ProjectName = dto.ProjectName,
            SupervisorName = dto.SupervisorName,
            RoundNumber = dto.RoundNumber,
            ScheduledAt = dto.ScheduledAt,
            Members = dto.Members.Select(m => new GroupMemberItem
            {
                UserId = m.UserId,
                FullName = m.FullName,
                Role = m.RoleInGroup
            }).ToList(),
            ChecklistItems = dto.ChecklistItems.Select(c => new ChecklistItemRow
            {
                ItemID = c.ItemId,
                ItemCode = c.ItemCode,
                ItemContent = c.ItemContent,
                MaxScore = c.MaxScore,
                Weight = c.Weight
            }).ToList(),
            ExistingEvaluationID = dto.ExistingEvaluationId,
            ExistingFeedbackContent = dto.ExistingFeedbackContent,
            ExistingScores = dto.ExistingScores.Select(s => new ExistingScoreRow
            {
                ItemID = s.ItemId,
                Score = s.Score,
                Comment = s.Comment
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // POST actions
    // -------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> SubmitEvaluation(EvaluationFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("EvaluationForm", model);

        var submitDto = new EvaluationSubmitDto
        {
            AssignmentId = model.AssignmentID,
            OverallFeedback = model.ExistingFeedbackContent ?? string.Empty
        };

        var success = await _lecturerService.SubmitEvaluationAsync(submitDto);
        if (success)
        {
            TempData["SuccessMessage"] = "Evaluation submitted successfully.";
            return RedirectToAction(nameof(ReviewAssignments));
        }

        ModelState.AddModelError("", "An error occurred while submitting the evaluation.");
        return View("EvaluationForm", model);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveFeedback(int feedbackId, string decision, string comments)
    {
        var success = await _lecturerService.ApproveFeedbackAsync(feedbackId, decision, comments);
        if (success)
        {
            TempData["SuccessMessage"] = $"Feedback {decision.ToLower()} successfully.";
            return RedirectToAction(nameof(FeedbackApprovals));
        }

        TempData["ErrorMessage"] = "An error occurred while processing the feedback approval.";
        return RedirectToAction(nameof(FeedbackApprovalDetail), new { id = feedbackId });
    }
}
