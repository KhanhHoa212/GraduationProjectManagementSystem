using System;
using System.Collections.Generic;
using GPMS.Domain.Enums;

namespace GPMS.Web.ViewModels.Lecturer
{
    // -------------------------------------------------------
    // Dashboard
    // -------------------------------------------------------
    public class LecturerDashboardViewModel
    {
        public int MentoringGroupsCount { get; set; }
        public int PendingApprovalsCount { get; set; }
        public int AssignedReviewsCount { get; set; }
        public int UpcomingDeadlinesCount { get; set; }
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
        public List<ScheduleItem> TodaysSchedule { get; set; } = new();
        public string GuidanceMessage { get; set; } = string.Empty;
    }

    public class RecentActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = "notifications";
        public string IconBgColor { get; set; } = "var(--fpt-orange)";
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
    }

    public class ScheduleItem
    {
        public DateTime StartTime { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public bool IsHighlight { get; set; }
        public string? MeetLink { get; set; }
        public string? ActionUrl { get; set; }
    }

    // -------------------------------------------------------
    // My Projects (Mentor)
    // -------------------------------------------------------
    public class MentoredProjectsViewModel
    {
        public List<MentoredGroupRow> Groups { get; set; } = new();
        public string? SearchQuery { get; set; }
    }

    public class MentoredGroupRow
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public List<string> MemberNames { get; set; } = new();
        public int CurrentRound { get; set; }
        public string RoundType { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ProgressPercent { get; set; }
        public DateTime? NextSessionAt { get; set; }
        public string? NextSessionLocation { get; set; }
        public int PendingFeedbackCount { get; set; }
    }

    // -------------------------------------------------------
    // Project Group Detail
    // -------------------------------------------------------
    public class ProjectGroupDetailViewModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
        public int? PendingFeedbackId { get; set; }
        public string? MessageGroupUrl { get; set; }
        public List<GroupMemberItem> Members { get; set; } = new();
        public List<ReviewRoundMilestone> Milestones { get; set; } = new();
        public MeetingInfoItem? NextMeeting { get; set; }
    }

    public class GroupMemberItem
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Member";
    }

    public class MeetingInfoItem
    {
        public DateTime ScheduledAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? MeetLink { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ReviewRoundMilestone
    {
        public int RoundId { get; set; }
        public int RoundNumber { get; set; }
        public int? SubmissionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string RoundType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public MentorGateStatus MentorGateStatus { get; set; } = MentorGateStatus.Pending;
        public string? MentorGateComment { get; set; }
        public bool HasSubmission { get; set; }
        public bool HasEvaluation { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmissionFileName { get; set; }
        public string? SubmissionUrl { get; set; }
        public decimal? SubmissionSizeMb { get; set; }
        public string? SubmittedByName { get; set; }
        public string? ReviewerName { get; set; }

        public string? FeedbackStatus { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public string? Location { get; set; }
        public string? MeetLink { get; set; }
    }

    // -------------------------------------------------------
    // Feedback Approvals (Mentor)
    // -------------------------------------------------------
    public class FeedbackApprovalsViewModel
    {
        public List<FeedbackApprovalRow> Approvals { get; set; } = new();
        public string? StatusFilter { get; set; }
    }

    public class FeedbackApprovalRow
    {
        public int FeedbackID { get; set; }
        public int EvaluationID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int RoundNumber { get; set; }

        public DateTime SubmittedAt { get; set; }
        public DateTime? AutoReleaseAt { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public string ApprovalStatusLabel => ApprovalStatus switch
        {
            ApprovalStatus.Approved => "Approved",
            ApprovalStatus.Rejected => "Rejected",
            _ => "Pending"
        };
        public string ApprovalStatusBadgeClass => ApprovalStatus switch
        {
            ApprovalStatus.Approved => "bg-success-subtle text-success",
            ApprovalStatus.Rejected => "bg-danger-subtle text-danger",
            _ => "bg-warning-subtle text-warning"
        };
    }

    // -------------------------------------------------------
    // Feedback Approval Detail (Mentor)
    // -------------------------------------------------------
    public class FeedbackApprovalDetailViewModel
    {
        public int FeedbackID { get; set; }
        public int EvaluationID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int GroupID { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewRoundName { get; set; } = string.Empty;
        public int CurrentRoundIndex { get; set; }
        public int TotalRounds { get; set; }

        public DateTime SubmittedAt { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public string FeedbackContent { get; set; } = string.Empty;
        public string? SupervisorComment { get; set; }
        public MentorGateStatus MentorGateStatus { get; set; } = MentorGateStatus.Pending;
        public string? MentorGateComment { get; set; }
        public List<GroupMemberItem> GroupMembers { get; set; } = new();
        public List<EvalDetailRow> Scores { get; set; } = new();
    }

    public class EvalDetailRow
    {
        public int ItemID { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public string ItemContent { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string? ItemType { get; set; }
        public string? Assessment { get; set; }
        public string? ReviewerComment { get; set; }
        public string? MentorComment { get; set; }
        public string? GradeDescription { get; set; }
        public List<RubricDescriptionViewModel> RubricDescriptions { get; set; } = new();
    }

    // -------------------------------------------------------
    // Review Assignments (Reviewer)
    // -------------------------------------------------------
    public class ReviewAssignmentsViewModel
    {
        public List<ReviewAssignmentRow> Assignments { get; set; } = new();
        public int PendingEvaluationsCount { get; set; }
        public int ScheduledTodayCount { get; set; }
        public int CompletedReviewsCount { get; set; }
    }

    public class ReviewAssignmentRow
    {
        public int AssignmentID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public int RoundNumber { get; set; }
        public string RoundType { get; set; } = string.Empty;
        public DateTime? ScheduledAt { get; set; }
        public string? Location { get; set; }
        public string? MeetLink { get; set; }
        public bool HasEvaluation { get; set; }
        public int? EvaluationID { get; set; }
        public string? StatusNote { get; set; }
        public string StatusLabel => !string.IsNullOrWhiteSpace(StatusNote) ? StatusNote : HasEvaluation ? "Completed" : (ScheduledAt.HasValue && ScheduledAt.Value < DateTime.Now ? "Overdue" : "Pending");
        public string StatusBadgeClass => !string.IsNullOrWhiteSpace(StatusNote) ? "bg-danger-subtle text-danger" : HasEvaluation ? "bg-success-subtle text-success" : (ScheduledAt.HasValue && ScheduledAt.Value < DateTime.Now ? "bg-danger-subtle text-danger" : "bg-warning-subtle text-warning");
        public string PrimaryActionLabel => StatusNote == "Needs Revision" ? "Revise Evaluation" : "Start Evaluation";
    }

    // -------------------------------------------------------
    // Evaluation Form (Reviewer)
    // -------------------------------------------------------
    public class EvaluationFormViewModel
    {
        public int AssignmentID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
        public int RoundNumber { get; set; }
        public string RoundType { get; set; } = string.Empty;
        public DateTime? ScheduledAt { get; set; }
        public string? SubmissionFileName { get; set; }
        public string? SubmissionUrl { get; set; }
        public List<GroupMemberItem> Members { get; set; } = new();
        public List<ChecklistItemRow> ChecklistItems { get; set; } = new();
        public int? ExistingEvaluationID { get; set; }
        public string? ExistingFeedbackContent { get; set; }
        public List<ExistingScoreRow> ExistingScores { get; set; } = new();
        public List<ScoreInputRow> CriteriaScores { get; set; } = new();
        public ApprovalStatus? FeedbackApprovalStatus { get; set; }
        public string? SupervisorComment { get; set; }
        public MentorGateStatus MentorGateStatus { get; set; } = MentorGateStatus.Pending;
        public string? MentorGateComment { get; set; }
        public bool CanEdit { get; set; } = true;
    }

    public class ChecklistItemRow
    {
        public int ItemID { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public string ItemContent { get; set; } = string.Empty;
        public string? ItemType { get; set; }
        public string? Section { get; set; }
        public List<RubricDescriptionViewModel> RubricDescriptions { get; set; } = new();
    }

    public class RubricDescriptionViewModel
    {
        public string GradeLevel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ExistingScoreRow
    {
        public int ItemID { get; set; }
        public string? Assessment { get; set; }
        public string? Comment { get; set; }
        public string? MentorComment { get; set; }
        public string? GradeDescription { get; set; }
    }

    public class ScoreInputRow
    {
        public int CriteriaId { get; set; }
        public string? Assessment { get; set; }
        public string? Comment { get; set; }
    }

    // -------------------------------------------------------
    // Schedule
    // -------------------------------------------------------
    public class LecturerScheduleViewModel
    {
        public int TodaySessionsCount { get; set; }
        public int OnlineSessionsCount { get; set; }
        public int OfflineSessionsCount { get; set; }
        public int UpcomingDeadlinesCount { get; set; }
        public List<ScheduleEntryViewModel> Entries { get; set; } = new();
        public List<DeadlineAlertViewModel> Deadlines { get; set; } = new();
    }

    public class ScheduleEntryViewModel
    {
        public string RoleLabel { get; set; } = string.Empty;
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public int RoundNumber { get; set; }
        public string RoundType { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public bool IsOnline { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? MeetLink { get; set; }
        public string? Guidance { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
    }

    public class DeadlineAlertViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueAt { get; set; }
        public string Severity { get; set; } = "info";
        public string ActionUrl { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
    }

    // -------------------------------------------------------
    // History
    // -------------------------------------------------------
    public class LecturerHistoryViewModel
    {
        public List<ReviewHistoryRow> ReviewHistory { get; set; } = new();
        public List<FeedbackHistoryRow> FeedbackHistory { get; set; } = new();
    }

    public class ReviewHistoryRow
    {
        public int EvaluationID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public int RoundNumber { get; set; }
        public string RoundType { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public string FeedbackPreview { get; set; } = string.Empty;
    }

    public class FeedbackHistoryRow
    {
        public int FeedbackID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int RoundNumber { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsVisibleToStudent { get; set; }
        public string? SupervisorComment { get; set; }
    }
}
