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
            MentoringGroupsCount = dto.MentoringGroupsCount,
            PendingApprovalsCount = dto.PendingApprovalsCount,
            AssignedReviewsCount = dto.AssignedReviewsCount,
            UpcomingDeadlinesCount = dto.UpcomingDeadlinesCount,
            RecentActivities = dto.RecentActivities.Select(a => new RecentActivityItem
            {
                Title = a.Title,
                Description = a.Description,
                Timestamp = a.Timestamp,
                Icon = a.Icon,
                IconBgColor = a.IconBgColor,
                ActionUrl = a.ActionUrl,
                ActionText = a.ActionText
            }).ToList(),
            TodaysSchedule = dto.TodaysSchedule.Select(s => new ScheduleItem
            {
                Title = s.Title,
                Location = s.Location,
                StartTime = s.StartTime,
                DurationMinutes = s.DurationMinutes,
                IsHighlight = s.IsHighlight
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
                Semester = p.Semester,
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
            Semester = dto.Semester,
            PendingFeedbackId = dto.PendingFeedbackId,
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
                ProjectName = f.ProjectName,
                ReviewerName = f.ReviewerName,
                RoundNumber = f.RoundNumber,
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
            ReviewRoundName = dto.ReviewRoundName,
            CurrentRoundIndex = dto.CurrentRoundIndex,
            TotalRounds = dto.TotalRounds,
            TotalScore = dto.TotalScore,
            MaxTotalScore = dto.MaxTotalScore,
            FeedbackContent = dto.FeedbackContent,
            GroupMembers = dto.Members.Select(m => new GroupMemberItem
            {
                UserId = m.UserId,
                FullName = m.FullName,
                Role = m.RoleInGroup
            }).ToList(),
            Scores = dto.Scores.Select(s => new EvalDetailRow
            {
                ItemCode = s.ItemCode,
                CriteriaName = s.CriteriaName,
                MaxScore = s.MaxScore,
                WeightPercentage = s.WeightPercentage,
                Score = s.Score,
                WeightedScore = s.WeightedScore
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
                RoundType = a.ReviewRoundName,
                ScheduledAt = a.ScheduledAt,
                Location = a.Location,
                HasEvaluation = a.HasEvaluation,
                EvaluationID = a.EvaluationId
            }).ToList(),
            PendingEvaluationsCount = dto.PendingEvaluationsCount,
            ScheduledTodayCount = dto.ScheduledTodayCount,
            CompletedReviewsCount = dto.CompletedReviewsCount
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
