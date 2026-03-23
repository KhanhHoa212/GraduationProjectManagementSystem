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

[Authorize(Roles = "Lecturer")]
public class LecturerController : Controller
{
    private readonly ILecturerService _lecturerService;

    public LecturerController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // -------------------------------------------------------
    // Dashboard
    // -------------------------------------------------------
    public async Task<IActionResult> Dashboard()
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var dto = await _lecturerService.GetDashboardDataAsync(userId);
        var vm = new LecturerDashboardViewModel
        {
            MentoringGroupsCount = dto.MentoringGroupsCount,
            PendingApprovalsCount = dto.PendingApprovalsCount,
            AssignedReviewsCount = dto.AssignedReviewsCount,
            UpcomingDeadlinesCount = dto.UpcomingDeadlinesCount,
            GuidanceMessage = dto.GuidanceMessage,
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
                IsHighlight = s.IsHighlight,
                MeetLink = s.MeetLink,
                ActionUrl = s.ActionUrl
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Projects
    // -------------------------------------------------------
    public async Task<IActionResult> Projects()
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var dto = await _lecturerService.GetMentoredProjectsAsync(userId);
        var vm = new MentoredProjectsViewModel
        {
            Groups = dto.Projects.Select(p => new MentoredGroupRow
            {
                GroupID = p.GroupId,
                GroupName = p.GroupName,
                ProjectName = p.ProjectName,
                ProjectCode = p.ProjectCode,
                MemberNames = p.MemberNames,
                CurrentRound = int.TryParse(new string(p.CurrentRound.Where(char.IsDigit).ToArray()), out var roundNumber) ? roundNumber : 0,
                RoundType = p.CurrentRound,
                Semester = p.Semester,
                Status = p.Status,
                ProgressPercent = p.ProgressPercent,
                NextSessionAt = p.NextSessionAt,
                NextSessionLocation = p.NextSessionLocation,
                PendingFeedbackCount = p.PendingFeedbackCount
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Project Group Detail
    // -------------------------------------------------------
    public async Task<IActionResult> ProjectGroupDetail(int id = 100)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var dto = await _lecturerService.GetProjectGroupDetailAsync(userId, id);
        var memberEmails = dto.Members
            .Select(m => m.Email)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email!)
            .Distinct()
            .ToList();
        var vm = new ProjectGroupDetailViewModel
        {
            GroupID = dto.GroupId,
            GroupName = dto.GroupName,
            ProjectName = dto.ProjectName,
            ProjectCode = dto.ProjectCode,
            Semester = dto.Semester,
            SupervisorName = dto.SupervisorName,
            PendingFeedbackId = dto.PendingFeedbackId,
            MessageGroupUrl = memberEmails.Any()
                ? $"mailto:{string.Join(";", memberEmails)}?subject={Uri.EscapeDataString($"[GPMS] Update for {dto.GroupName}")}"
                : null,
            Members = dto.Members.Select(m => new GroupMemberItem
            {
                UserId = m.UserId,
                FullName = m.FullName,
                Role = m.RoleInGroup,
                Email = m.Email ?? string.Empty
            }).ToList(),
            Milestones = dto.Milestones.Select(m => new ReviewRoundMilestone
            {
                RoundId = m.RoundId,
                RoundNumber = m.RoundNumber,
                Title = m.Title,
                RoundType = m.RoundType,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Deadline = m.Deadline,
                Status = m.Status,
                MentorGateStatus = m.MentorGateStatus,
                MentorGateComment = m.MentorGateComment,
                HasSubmission = m.SubmittedAt.HasValue,
                HasEvaluation = m.FeedbackStatus != null,
                SubmittedAt = m.SubmittedAt,
                SubmissionFileName = m.SubmissionFileName,
                SubmissionUrl = m.ReportDocumentUrl,
                SubmissionSizeMb = m.SubmissionSizeMb,
                SubmittedByName = m.SubmittedByName,
                ReviewerName = m.ReviewerName,

                FeedbackStatus = m.FeedbackStatus,
                ScheduledAt = m.ScheduledAt,
                Location = m.Location,
                MeetLink = m.MeetLink
            }).ToList(),
            NextMeeting = dto.NextMeeting == null ? null : new MeetingInfoItem
            {
                ScheduledAt = dto.NextMeeting.ScheduledAt,
                Title = dto.NextMeeting.Title,
                Location = dto.NextMeeting.Location,
                MeetLink = dto.NextMeeting.MeetLink,
                IsOnline = dto.NextMeeting.IsOnline
            }
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewRoundGate(int groupId, int roundId, MentorGateStatus decision, string? progressComment)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var success = await _lecturerService.ReviewRoundGateAsync(userId, groupId, roundId, decision, progressComment);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Mentor gate updated successfully."
            : "Unable to update mentor gate for this round.";
        return RedirectToAction(nameof(ProjectGroupDetail), new { id = groupId });
    }

    // -------------------------------------------------------
    // Feedback Approvals
    // -------------------------------------------------------
    public async Task<IActionResult> FeedbackApprovals()
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var dto = await _lecturerService.GetPendingApprovalsAsync(userId);
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
                AutoReleaseAt = f.AutoReleaseAt,
                ApprovalStatus = f.ApprovalStatus
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

            SubmittedAt = dto.SubmittedAt,
            ApprovalStatus = dto.ApprovalStatus,
            SupervisorComment = dto.SupervisorComment,
            MentorGateStatus = dto.MentorGateStatus,
            MentorGateComment = dto.MentorGateComment,
            FeedbackContent = dto.FeedbackContent,
            GroupMembers = dto.Members.Select(m => new GroupMemberItem
            {
                UserId = m.UserId,
                FullName = m.FullName,
                Role = m.RoleInGroup,
                Email = m.Email ?? string.Empty
            }).ToList(),
            Scores = dto.Scores.Select(s => new EvalDetailRow
            {
                ItemID = s.ItemId,
                ItemCode = s.ItemCode,
                ItemName = s.ItemName,
                ItemContent = s.ItemContent,
                Section = s.Section,
                ItemType = s.ItemType,
                Assessment = s.Assessment,
                ReviewerComment = s.ReviewerComment,
                MentorComment = s.MentorComment,
                GradeDescription = s.GradeDescription,
                RubricDescriptions = s.RubricDescriptions.Select(r => new RubricDescriptionViewModel
                {
                    GradeLevel = r.GradeLevel,
                    Description = r.Description
                }).ToList()
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Review Assignments
    // -------------------------------------------------------
    public async Task<IActionResult> ReviewAssignments()
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var dto = await _lecturerService.GetReviewAssignmentsAsync(userId);
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
                MeetLink = a.MeetLink,
                HasEvaluation = a.HasEvaluation,
                EvaluationID = a.EvaluationId,
                StatusNote = a.StatusNote
            }).ToList(),
            PendingEvaluationsCount = dto.PendingEvaluationsCount,
            ScheduledTodayCount = dto.ScheduledTodayCount,
            CompletedReviewsCount = dto.CompletedReviewsCount
        };
        return View(vm);
    }

    public async Task<IActionResult> Schedule()
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var dto = await _lecturerService.GetScheduleAsync(userId);
        var vm = new LecturerScheduleViewModel
        {
            TodaySessionsCount = dto.TodaySessionsCount,
            OnlineSessionsCount = dto.OnlineSessionsCount,
            OfflineSessionsCount = dto.OfflineSessionsCount,
            UpcomingDeadlinesCount = dto.UpcomingDeadlinesCount,
            Entries = dto.Entries.Select(e => new ScheduleEntryViewModel
            {
                RoleLabel = e.RoleLabel,
                GroupID = e.GroupId,
                GroupName = e.GroupName,
                ProjectName = e.ProjectName,
                RoundNumber = e.RoundNumber,
                RoundType = e.RoundType,
                ScheduledAt = e.ScheduledAt,
                IsOnline = e.IsOnline,
                Location = e.Location,
                MeetLink = e.MeetLink,
                Guidance = e.Guidance,
                ActionUrl = e.ActionUrl
            }).ToList(),
            Deadlines = dto.Deadlines.Select(d => new DeadlineAlertViewModel
            {
                Title = d.Title,
                Description = d.Description,
                DueAt = d.DueAt,
                Severity = d.Severity,
                ActionUrl = d.ActionUrl,
                ActionText = d.ActionText
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> History()
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var dto = await _lecturerService.GetHistoryAsync(userId);
        var vm = new LecturerHistoryViewModel
        {
            ReviewHistory = dto.ReviewHistory.Select(r => new ReviewHistoryRow
            {
                EvaluationID = r.EvaluationId,
                GroupID = r.GroupId,
                GroupName = r.GroupName,
                ProjectName = r.ProjectName,
                RoundNumber = r.RoundNumber,
                RoundType = r.RoundType,

                SubmittedAt = r.SubmittedAt,
                ApprovalStatus = r.ApprovalStatus,
                FeedbackPreview = r.FeedbackPreview
            }).ToList(),
            FeedbackHistory = dto.FeedbackHistory.Select(f => new FeedbackHistoryRow
            {
                FeedbackID = f.FeedbackId,
                GroupID = f.GroupId,
                GroupName = f.GroupName,
                ProjectName = f.ProjectName,
                ReviewerName = f.ReviewerName,
                RoundNumber = f.RoundNumber,
                ApprovalStatus = f.ApprovalStatus,
                UpdatedAt = f.UpdatedAt,
                IsVisibleToStudent = f.IsVisibleToStudent,
                SupervisorComment = f.SupervisorComment
            }).ToList()
        };
        return View(vm);
    }

    // -------------------------------------------------------
    // Evaluation Form
    // -------------------------------------------------------
    public async Task<IActionResult> EvaluationForm(int id = 1)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var dto = await _lecturerService.GetEvaluationFormAsync(userId, id);
        if (dto == null)
            return NotFound();

        var vm = new EvaluationFormViewModel
        {
            AssignmentID = dto.AssignmentId,
            GroupID = dto.GroupId,
            GroupName = dto.GroupName,
            ProjectName = dto.ProjectName,
            SupervisorName = dto.SupervisorName,
            RoundNumber = dto.RoundNumber,
            RoundType = dto.ReviewRoundName,
            ScheduledAt = dto.ScheduledAt,
            SubmissionFileName = dto.SubmissionFileName,
            SubmissionUrl = dto.SubmissionUrl,
            Members = dto.Members.Select(m => new GroupMemberItem
            {
                UserId = m.UserId,
                FullName = m.FullName,
                Role = m.RoleInGroup,
                Email = m.Email ?? string.Empty
            }).ToList(),
            ExistingEvaluationID = dto.ExistingEvaluationId,
            ExistingFeedbackContent = dto.ExistingFeedbackContent,
            FeedbackApprovalStatus = dto.FeedbackApprovalStatus,
            SupervisorComment = dto.SupervisorComment,
            MentorGateStatus = dto.MentorGateStatus,
            MentorGateComment = dto.MentorGateComment,
            CanEdit = dto.CanEdit,
            ExistingScores = dto.ExistingScores.Select(s => new ExistingScoreRow
            {
                ItemID = s.ItemId,
                Assessment = s.Assessment,
                Comment = s.Comment,
                MentorComment = s.MentorComment,
                GradeDescription = s.GradeDescription
            }).ToList(),
            CriteriaScores = dto.ChecklistItems.Select(c =>
            {
                var existingScore = dto.ExistingScores.FirstOrDefault(s => s.ItemId == c.ItemId);
                return new ScoreInputRow
                {
                    CriteriaId = c.ItemId,
                    Assessment = existingScore?.Assessment,
                    Comment = existingScore?.Comment
                };
            }).ToList()
        };
        vm.ChecklistItems = dto.ChecklistItems.Select(c => new ChecklistItemRow
        {
            ItemID = c.ItemId,
            ItemCode = c.ItemCode,
            ItemName = c.ItemName,
            ItemContent = c.ItemContent,
            ItemType = c.ItemType,
            Section = c.Section,
            RubricDescriptions = c.RubricDescriptions.Select(r => new RubricDescriptionViewModel
            {
                GradeLevel = r.GradeLevel,
                Description = r.Description
            }).ToList()
        }).ToList();
        return View(vm);
    }

    // -------------------------------------------------------
    // POST actions
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitEvaluation(EvaluationFormViewModel model)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!ModelState.IsValid)
            return RedirectToAction(nameof(EvaluationForm), new { id = model.AssignmentID });

        var submitDto = new EvaluationSubmitDto
        {
            AssignmentId = model.AssignmentID,
            OverallFeedback = model.ExistingFeedbackContent ?? string.Empty,
            CriteriaScores = model.CriteriaScores.Where(s => s != null).Select(s => new ScoreInputDto
            {
                CriteriaId = s.CriteriaId,
                Assessment = s.Assessment,
                Comment = s.Comment
            }).ToList()
        };

        var result = await _lecturerService.SubmitEvaluationAsync(userId, submitDto);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Evaluation submitted successfully.";
            return RedirectToAction(nameof(ReviewAssignments));
        }

        TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(result.ErrorMessage)
            ? "Unable to submit evaluation. Please check the assignment and score inputs."
            : result.ErrorMessage;
        return RedirectToAction(nameof(EvaluationForm), new { id = model.AssignmentID });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveFeedback(FeedbackApprovalDetailViewModel model, string decision, string comments)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var dto = new FeedbackApprovalDecisionDto
        {
            FeedbackId = model.FeedbackID,
            Decision = decision,
            OverallFeedbackContent = model.FeedbackContent ?? string.Empty,
            SupervisorComment = comments,
            ItemComments = model.Scores.Select(s => new MentorChecklistCommentDto
            {
                ItemId = s.ItemID,
                MentorComment = s.MentorComment
            }).ToList()
        };

        var success = await _lecturerService.ApproveFeedbackAsync(userId, dto);
        if (success)
        {
            TempData["SuccessMessage"] = $"Feedback {decision.ToLower()} successfully.";
            return RedirectToAction(nameof(FeedbackApprovals));
        }

        TempData["ErrorMessage"] = "An error occurred while processing the feedback approval.";
        return RedirectToAction(nameof(FeedbackApprovalDetail), new { id = model.FeedbackID });
    }
}
