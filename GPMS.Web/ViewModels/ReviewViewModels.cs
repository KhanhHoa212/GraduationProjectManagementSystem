using System;
using System.Collections.Generic;
using GPMS.Domain.Enums;

namespace GPMS.Web.ViewModels
{
    public class AssignedGroupViewModel
    {
        public int ReviewRoundID { get; set; }
        public int GroupID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public RoundType RoundType { get; set; }
        public int RoundNumber { get; set; }
        public DateTime StartDate { get; set; }
        public string? RoomCode { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }

    public class ReviewerDashboardViewModel
    {
        public List<AssignedGroupViewModel> AssignedGroups { get; set; } = new();
    }

    public class EvaluationItemViewModel
    {
        public int ItemID { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemContent { get; set; } = string.Empty;
        public string? Assessment { get; set; }
        public string? Comment { get; set; }
    }

    public class EvaluationFormViewModel
    {
        public int EvaluationID { get; set; }
        public int ReviewRoundID { get; set; }
        public int GroupID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string ChecklistTitle { get; set; } = string.Empty;
        public EvaluationStatus Status { get; set; }
        public List<EvaluationItemViewModel> Items { get; set; } = new();
        public string? GeneralComment { get; set; }
    }

    public class PendingFeedbackViewModel
    {
        public int FeedbackID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public string RoundName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }

    public class SupervisorFeedbackReviewViewModel
    {
        public List<PendingFeedbackViewModel> PendingFeedbacks { get; set; } = new();
    }
    
    public class ApproveFeedbackRequest
    {
        public int FeedbackID { get; set; }
        public ApprovalStatus Status { get; set; }
        public string? SupervisorComment { get; set; }
    }

    public class StudentFeedbackDetailsViewModel
    {
        public int FeedbackID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string RoundName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public List<EvaluationItemViewModel> Items { get; set; } = new();
    }

    public class StudentFeedbackHistoryViewModel
    {
        public List<StudentFeedbackDetailsViewModel> Feedbacks { get; set; } = new();
    }
}
