using System;
using System.Collections.Generic;

namespace GPMS.Web.ViewModels
{
    public class SupervisorDashboardViewModel
    {
        public int TotalGroups { get; set; }
        public int PendingReviews { get; set; }
        public int FeedbackApproved { get; set; }
        public List<SupervisedGroupStatusViewModel> SupervisedGroups { get; set; } = new();
        public List<UpcomingDeadlineViewModel> UpcomingDeadlines { get; set; } = new();
    }

    public class SupervisedGroupStatusViewModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public List<string> MemberNames { get; set; } = new();
        public int ProgressPercentage { get; set; }
        public string Status { get; set; } = "On Track";
        public string StatusColorClass { get; set; } = "bg-emerald-50 text-emerald-600 border-emerald-100/50";
    }

    public class UpcomingDeadlineViewModel
    {
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ColorClass { get; set; } = "bg-gray-300";
        public List<string> RelatedGroups { get; set; } = new();
    }
}
