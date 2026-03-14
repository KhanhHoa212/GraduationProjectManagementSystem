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

    public async Task<LecturerDashboardDto> GetDashboardDataAsync(string lecturerId)
    {
        var groups = await _groupRepo.GetBySupervisorAsync(lecturerId);
        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId);
        var assignments = await _assignmentRepo.GetByReviewerAsync(lecturerId);

        return new LecturerDashboardDto
        {
            MentoringGroupsCount = groups.Count(),
            PendingApprovalsCount = pendingFeedbacks.Count(),
            AssignedReviewsCount = assignments.Count(),
            UpcomingDeadlinesCount = 3, // Mocked

            RecentActivities = new List<DashboardActivityItemDto>
            {
                new DashboardActivityItemDto
                {
                    Title = "Group G04 - AI Ethics",
                    Description = "New final project report submitted for your approval.",
                    Timestamp = DateTime.Now.AddMinutes(-14),
                    Icon = "note_add",
                    IconBgColor = "var(--fpt-orange)",
                    ActionUrl = "/Lecturer/FeedbackApprovalDetail",
                    ActionText = "Review Now"
                },
                new DashboardActivityItemDto
                {
                    Title = "Review Assignment",
                    Description = "You were assigned to review \"Smart Irrigation IoT\" by Group G12.",
                    Timestamp = DateTime.Now.AddHours(-2),
                    Icon = "chat",
                    IconBgColor = "#6C757D",
                    ActionUrl = null,
                    ActionText = null
                }
            },
            TodaysSchedule = new List<DashboardScheduleItemDto>
            {
                new DashboardScheduleItemDto
                {
                    Title = "Mentoring Session G04",
                    Location = "Virtual Room A",
                    DurationMinutes = 45,
                    StartTime = DateTime.Today.AddHours(10),
                    IsHighlight = true
                },
                new DashboardScheduleItemDto
                {
                    Title = "Peer Review Meeting",
                    Location = "Office 302",
                    DurationMinutes = 60,
                    StartTime = DateTime.Today.AddHours(14),
                    IsHighlight = false
                }
            }
        };
    }

    public async Task<LecturerProjectsDto> GetMentoredProjectsAsync(string lecturerId)
    {
        var groups = await _groupRepo.GetBySupervisorAsync(lecturerId);
        var dto = new LecturerProjectsDto();

        foreach (var group in groups)
        {
            dto.Projects.Add(new LecturerProjectItemDto
            {
                GroupId = group.GroupID,
                ProjectName = group.Project?.ProjectName ?? "N/A",
                GroupName = group.GroupName,
                Semester = "SP25",
                SupervisorRole = "Main",
                MemberNames = group.GroupMembers.Select(m => m.User?.FullName ?? "Unknown").ToList(),
                CurrentRound = "Round 1",
                Status = "Active",
                ProgressPercent = 33
            });
        }

        return dto;
    }

    public async Task<LecturerProjectGroupDetailDto> GetProjectGroupDetailAsync(int groupId)
    {
        var group = await _groupRepo.GetByIdAsync(groupId);
        if (group == null) throw new Exception("Group not found");

        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(string.Empty);
        var groupFeedback = pendingFeedbacks
            .Where(f => f.Evaluation?.GroupID == groupId)
            .OrderByDescending(f => f.CreatedAt)
            .FirstOrDefault();

        return new LecturerProjectGroupDetailDto
        {
            GroupId = group.GroupID,
            GroupName = group.GroupName,
            ProjectName = group.Project?.ProjectName ?? "N/A",
            Semester = "SP25",
            PendingFeedbackId = groupFeedback?.FeedbackID,
            Members = group.GroupMembers.Select(m => new StudentMemberDto
            {
                UserId = m.UserID,
                FullName = m.User?.FullName ?? "Unknown",
                RoleInGroup = m.RoleInGroup.ToString(),
                AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(m.User?.FullName ?? "U")}&background=E5E7EB&color=374151"
            }).ToList()
        };
    }

    public async Task<LecturerFeedbackApprovalsDto> GetPendingApprovalsAsync(string lecturerId)
    {
        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId);
        var dto = new LecturerFeedbackApprovalsDto();

        foreach (var f in pendingFeedbacks)
        {
            dto.PendingFeedbacks.Add(new PendingFeedbackItemDto
            {
                FeedbackId = f.FeedbackID,
                EvaluationId = f.EvaluationID,
                GroupName = f.Evaluation?.Group?.GroupName ?? "N/A",
                ProjectName = f.Evaluation?.Group?.Project?.ProjectName ?? "N/A",
                ReviewRoundName = f.Evaluation?.ReviewRound?.RoundNumber.ToString() ?? "N/A",
                ReviewerName = f.Evaluation?.Reviewer?.FullName ?? "N/A",
                SubmittedAt = f.CreatedAt,
                AutoReleaseAt = f.CreatedAt.AddHours(48),
                ApprovalStatus = "Pending"
            });
        }

        return dto;
    }

    public async Task<LecturerFeedbackApprovalDetailDto> GetFeedbackApprovalDetailAsync(int feedbackId)
    {
        var feedback = await _feedbackRepo.GetByIdWithDetailsAsync(feedbackId);
        if (feedback == null) throw new Exception("Feedback not found");

        var evaluation = feedback.Evaluation;
        var group = evaluation?.Group;

        var scores = evaluation?.EvaluationDetails?.Select(d => new EvaluationScoreItemDto
        {
            CriteriaName = d.Item?.ItemContent ?? "Unknown",
            Score = d.Score,
            MaxScore = d.Item?.MaxScore ?? 10,
            WeightPercentage = d.Item != null ? (decimal)d.Item.Weight : 100m
        }).ToList() ?? new List<EvaluationScoreItemDto>();

        var members = group?.GroupMembers?.Select(m => new StudentMemberDto
        {
            UserId = m.UserID,
            FullName = m.User?.FullName ?? "Unknown",
            RoleInGroup = m.RoleInGroup.ToString(),
            AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(m.User?.FullName ?? "U")}&background=E5E7EB&color=374151"
        }).ToList() ?? new List<StudentMemberDto>();

        return new LecturerFeedbackApprovalDetailDto
        {
            FeedbackId = feedback.FeedbackID,
            EvaluationId = feedback.EvaluationID,
            GroupName = group?.GroupName ?? "N/A",
            GroupId = group?.GroupID ?? 0,
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
    }

    public async Task<LecturerReviewAssignmentsDto> GetReviewAssignmentsAsync(string reviewerId)
    {
        var assignments = await _assignmentRepo.GetByReviewerAsync(reviewerId);
        var dto = new LecturerReviewAssignmentsDto();

        foreach (var a in assignments)
        {
            var session = a.Group?.ReviewSessions?.FirstOrDefault(rs => rs.ReviewRoundID == a.ReviewRoundID);
            dto.Assignments.Add(new ReviewAssignmentItemDto
            {
                AssignmentId = a.AssignmentID,
                GroupId = a.GroupID,
                GroupName = a.Group?.GroupName ?? "N/A",
                ProjectName = a.Group?.Project?.ProjectName ?? "N/A",
                ReviewRoundName = a.ReviewRound?.RoundNumber.ToString() ?? "N/A",
                RoundNumber = a.ReviewRound?.RoundNumber ?? 0,
                ScheduledAt = session?.ScheduledAt,
                Location = session?.Room?.RoomCode,
                IsOnline = false, // RoomType enum: Classroom, Lab, Hall — no Online value
                HasEvaluation = false,
                EvaluationId = null
            });
        }

        dto.PendingEvaluationsCount = dto.Assignments.Count(a => !a.HasEvaluation);
        dto.ScheduledTodayCount = dto.Assignments.Count(a => a.ScheduledAt?.Date == DateTime.Today);
        dto.CompletedReviewsCount = dto.Assignments.Count(a => a.HasEvaluation);

        return dto;
    }

    public Task<LecturerEvaluationFormDto> GetEvaluationFormAsync(int assignmentId)
    {
        // Stub — real implementation would load from DB
        var dto = new LecturerEvaluationFormDto
        {
            AssignmentId = assignmentId,
            GroupName = "G01",
            ProjectName = "Stubbed Project",
            ReviewRoundName = "Round 1",
            RoundNumber = 1,
            SupervisorName = "Prof. John Doe",
            ScheduledAt = DateTime.Now.AddDays(2),
            Members = new List<StudentMemberDto>
            {
                new StudentMemberDto { FullName = "Alice", RoleInGroup = "Leader", AvatarUrl = "https://ui-avatars.com/api/?name=AL&background=E5E7EB&color=374151" }
            },
            ChecklistItems = new List<ChecklistItemDto>
            {
                new ChecklistItemDto { ItemId = 1, ItemCode = "C1", ItemContent = "Criteria 1", MaxScore = 50, Weight = 50 },
                new ChecklistItemDto { ItemId = 2, ItemCode = "C2", ItemContent = "Criteria 2", MaxScore = 50, Weight = 50 }
            }
        };
        return Task.FromResult(dto);
    }

    public Task<bool> SubmitEvaluationAsync(EvaluationSubmitDto model)
    {
        // TODO: Map to entity and save
        return Task.FromResult(true);
    }

    public async Task<bool> ApproveFeedbackAsync(int feedbackId, string decision, string comments)
    {
        var feedback = await _feedbackRepo.GetByIdAsync(feedbackId);
        if (feedback == null) return false;

        if (decision == "ApproveWithEdits" && !string.IsNullOrEmpty(comments))
        {
            feedback.Content = comments;
        }

        await _feedbackRepo.SaveChangesAsync();
        return true;
    }
}
