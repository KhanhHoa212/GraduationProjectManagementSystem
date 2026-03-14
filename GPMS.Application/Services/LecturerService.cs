using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Application.DTOs.Lecturer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class LecturerService : ILecturerService
{
    private readonly IProjectGroupRepository _groupRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IReviewerAssignmentRepository _assignmentRepo;
    private readonly IFeedbackRepository _feedbackRepo;
    private readonly IReviewRoundRepository _roundRepo;
    private readonly IEvaluationRepository _evaluationRepo;

    public LecturerService(
        IProjectGroupRepository groupRepo,
        IProjectRepository projectRepo,
        IReviewerAssignmentRepository assignmentRepo,
        IFeedbackRepository feedbackRepo,
        IReviewRoundRepository roundRepo,
        IEvaluationRepository evaluationRepo)
    {
        _groupRepo = groupRepo;
        _projectRepo = projectRepo;
        _assignmentRepo = assignmentRepo;
        _feedbackRepo = feedbackRepo;
        _roundRepo = roundRepo;
        _evaluationRepo = evaluationRepo;
    }

    public async Task<LecturerDashboardViewModel> GetDashboardDataAsync(string lecturerId)
    {
        // TODO: Implement actual data fetching via repositories
        var groups = await _groupRepo.GetBySupervisorAsync(lecturerId);
        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId);
        var assignments = await _assignmentRepo.GetByReviewerAsync(lecturerId);

        return new LecturerDashboardViewModel
        {
            MentoringGroupsCount = groups.Count(),
            PendingApprovalsCount = pendingFeedbacks.Count(),
            AssignedReviewsCount = assignments.Count(),
            UpcomingDeadlinesCount = 3, // Mocked for now

            RecentActivities = new List<DashboardActivityItem>
            {
                new DashboardActivityItem
                {
                    Title = "Group G04 - AI Ethics",
                    Description = "New final project report submitted for your approval.",
                    Timestamp = DateTime.Now.AddMinutes(-14),
                    Icon = "note_add",
                    IconBgColor = "var(--fpt-orange)",
                    ActionUrl = "/Lecturer/FeedbackApprovalDetail",
                    ActionText = "Review Now"
                },
                new DashboardActivityItem
                {
                    Title = "Review Assignment",
                    Description = "You were assigned to review \"Smart Irrigation IoT\" by Group G12.",
                    Timestamp = DateTime.Now.AddHours(-2),
                    Icon = "chat",
                    IconBgColor = "#6C757D", // Secondary gray
                    ActionUrl = null,
                    ActionText = null
                }
            },
            TodaysSchedule = new List<DashboardScheduleItem>
            {
                new DashboardScheduleItem
                {
                    Title = "Mentoring Session G04",
                    Location = "Virtual Room A",
                    DurationMinutes = 45,
                    StartTime = DateTime.Today.AddHours(10), // 10:00 AM
                    IsHighlight = true
                },
                new DashboardScheduleItem
                {
                    Title = "Peer Review Meeting",
                    Location = "Office 302",
                    DurationMinutes = 60,
                    StartTime = DateTime.Today.AddHours(14), // 02:00 PM
                    IsHighlight = false
                }
            }
        };
    }

    public async Task<LecturerProjectsViewModel> GetMentoredProjectsAsync(string lecturerId)
    {
        var groups = await _groupRepo.GetBySupervisorAsync(lecturerId);
        var vm = new LecturerProjectsViewModel();

        foreach (var group in groups)
        {
            vm.Projects.Add(new LecturerProjectItem
            {
                GroupId = group.GroupID,
                ProjectName = group.Project?.ProjectName ?? "N/A",
                GroupName = group.GroupName,
                Semester = "SP25", // Mocked or get from Project.Semester
                SupervisorRole = "Main",
                MemberNames = group.GroupMembers.Select(m => m.User?.FullName ?? "Unknown").ToList(),
                CurrentRound = "Round 1",
                Status = "Active"
            });
        }
        
        return vm;
    }

    public async Task<LecturerProjectGroupDetailViewModel> GetProjectGroupDetailAsync(int groupId)
    {
        var group = await _groupRepo.GetByIdAsync(groupId);
        if (group == null) throw new Exception("Group not found");

        // Find a pending feedback for this group so the view can link to the correct FeedbackID
        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(string.Empty);
        var groupFeedback = pendingFeedbacks
            .Where(f => f.Evaluation?.GroupID == groupId)
            .OrderByDescending(f => f.CreatedAt)
            .FirstOrDefault();

        var vm = new LecturerProjectGroupDetailViewModel
        {
            GroupId = group.GroupID,
            GroupName = group.GroupName,
            ProjectName = group.Project?.ProjectName ?? "N/A",
            Semester = "SP25",
            PendingFeedbackId = groupFeedback?.FeedbackID,
            Members = group.GroupMembers.Select(m => new StudentMemberInfo
            {
                UserId = m.UserID,
                FullName = m.User?.FullName ?? "Unknown",
                RoleInGroup = m.RoleInGroup.ToString(),
                AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(m.User?.FullName ?? "U")}&background=E5E7EB&color=374151"
            }).ToList()
        };

        return vm;
    }

    public async Task<LecturerFeedbackApprovalsViewModel> GetPendingApprovalsAsync(string lecturerId)
    {
        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId);
        var vm = new LecturerFeedbackApprovalsViewModel();

        foreach (var f in pendingFeedbacks)
        {
            vm.PendingFeedbacks.Add(new PendingFeedbackItem
            {
                FeedbackId = f.FeedbackID,
                EvaluationId = f.EvaluationID,
                GroupName = f.Evaluation?.Group?.GroupName ?? "N/A",
                ProjectName = f.Evaluation?.Group?.Project?.ProjectName ?? "N/A",
                ReviewRoundName = f.Evaluation?.ReviewRound?.RoundNumber.ToString() ?? "N/A",
                ReviewerName = f.Evaluation?.Reviewer?.FullName ?? "N/A",
                SubmittedAt = f.CreatedAt,
                AutoReleaseAt = f.CreatedAt.AddHours(48), // Assuming 48 hr logic
                ApprovalStatus = "Pending"
            });
        }

        return vm;
    }

    public async Task<LecturerFeedbackApprovalDetailViewModel> GetFeedbackApprovalDetailAsync(int feedbackId)
    {
        var feedback = await _feedbackRepo.GetByIdWithDetailsAsync(feedbackId);
        if (feedback == null) throw new Exception("Feedback not found");

        var evaluation = feedback.Evaluation;
        var group = evaluation?.Group;

        var scores = evaluation?.EvaluationDetails?.Select(d => new EvaluationScoreItem
        {
            CriteriaName = d.Item?.ItemContent ?? "Unknown",
            Score = d.Score,
            MaxScore = d.Item?.MaxScore ?? 10,
            WeightPercentage = d.Item != null ? (decimal)d.Item.Weight : 100m
        }).ToList() ?? new List<EvaluationScoreItem>();

        var members = group?.GroupMembers?.Select(m => new StudentMemberInfo
        {
            UserId = m.UserID,
            FullName = m.User?.FullName ?? "Unknown",
            RoleInGroup = m.RoleInGroup.ToString(),
            AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(m.User?.FullName ?? "U")}&background=E5E7EB&color=374151"
        }).ToList() ?? new List<StudentMemberInfo>();

        var vm = new LecturerFeedbackApprovalDetailViewModel
        {
            FeedbackId = feedback.FeedbackID,
            EvaluationId = feedback.EvaluationID,
            GroupName = group?.GroupName ?? "N/A",
            GroupIdString = group?.GroupID.ToString() ?? "N/A",
            ReviewRoundName = evaluation?.ReviewRound?.RoundNumber.ToString() ?? "N/A",
            CurrentRoundIndex = evaluation?.ReviewRound?.RoundNumber ?? 1,
            TotalRounds = 4,
            ReviewerName = evaluation?.Reviewer?.FullName ?? "N/A",
            FeedbackContent = feedback.Content,
            TotalScore = evaluation?.TotalScore ?? 0,
            MaxTotalScore = scores.Any() ? scores.Sum(s => s.MaxScore) : 10.0m,
            Scores = scores,
            Members = members
        };

        return vm;
    }

    public async Task<LecturerReviewAssignmentsViewModel> GetReviewAssignmentsAsync(string reviewerId)
    {
        var assignments = await _assignmentRepo.GetByReviewerAsync(reviewerId);
        var vm = new LecturerReviewAssignmentsViewModel();

        foreach (var a in assignments)
        {
            vm.Assignments.Add(new ReviewAssignmentItem
            {
                AssignmentId = a.AssignmentID,
                GroupId = a.GroupID,
                GroupName = a.Group?.GroupName ?? "N/A",
                ProjectName = a.Group?.Project?.ProjectName ?? "N/A",
                ReviewRoundName = a.ReviewRound?.RoundNumber.ToString() ?? "N/A",
                ScheduledAt = DateTime.Now, // Need session info
                Location = "Room 101",
                IsOnline = false,
                EvaluationStatus = "Pending"
            });
        }

        return vm;
    }

    public async Task<LecturerEvaluationFormViewModel> GetEvaluationFormAsync(int assignmentId)
    {
        // Basic stub
        return new LecturerEvaluationFormViewModel
        {
            EvaluationId = assignmentId, // Using assignmentId for simple stub matching
            GroupName = "A",
            ProjectName = "Stubbed Project",
            ReviewRoundName = "Round 1",
            SupervisorName = "Prof. John Doe",
            DefenseDate = DateTime.Now.AddDays(2),
            Status = "Pending",
            Members = new List<StudentMemberInfo>
            {
                new StudentMemberInfo { FullName = "Alice", RoleInGroup = "Leader", AvatarUrl = "https://ui-avatars.com/api/?name=AL&background=E5E7EB&color=374151" }
            },
            Documents = new List<ProjectDocument>
            {
                new ProjectDocument { FileName = "Report.pdf", DocumentType = "Report", SizeInBytes = 1048576, UploadedAt = DateTime.Now }
            },
            Criteria = new List<EvaluationCriterion>
            {
                new EvaluationCriterion { Id = 1, Name = "Criteria 1", Description = "Desc 1", MaxScore = 50 },
                new EvaluationCriterion { Id = 2, Name = "Criteria 2", Description = "Desc 2", MaxScore = 50 }
            }
        };
    }

    public async Task<bool> SubmitEvaluationAsync(LecturerEvaluationFormViewModel model)
    {
        // TODO: Map to entity and save via _evaluationRepo and _feedbackRepo if evaluating.
        // For now, return true simulating a successful save.
        return await Task.FromResult(true);
    }

    public async Task<bool> ApproveFeedbackAsync(int feedbackId, string decision, string comments)
    {
        var feedback = await _feedbackRepo.GetByIdAsync(feedbackId);
        if (feedback == null) return false;

        if (decision == "Approve" || decision == "ApproveWithEdits")
        {
            if (decision == "ApproveWithEdits" && !string.IsNullOrEmpty(comments))
            {
                feedback.Content = comments;
            }
            // Update status or release flag
            // feedback.Status = "Approved"; // assuming property exists
        }
        else if (decision == "Reject")
        {
            // feedback.Status = "Rejected"; // assuming property exists
            // log comment reason
        }

        await _feedbackRepo.SaveChangesAsync();
        return true;
    }
}
