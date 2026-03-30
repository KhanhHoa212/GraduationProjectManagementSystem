using System;
using GPMS.Application.DTOs.Lecturer;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;

namespace GPMS.Application.Services;

public class LecturerService : ILecturerService
{
    private readonly IProjectGroupRepository _groupRepo;
    private readonly IReviewerAssignmentRepository _assignmentRepo;
    private readonly IFeedbackRepository _feedbackRepo;
    private readonly IReviewRoundRepository _roundRepo;
    private readonly IEvaluationRepository _evaluationRepo;
    private readonly IMentorRoundReviewRepository _mentorRoundReviewRepo;
    private readonly INotificationRepository _notificationRepo;
    private readonly ILecturerScheduleService _lecturerScheduleService;
    private readonly ILecturerWorkflowService _lecturerWorkflowService;
    private readonly ISubmissionAccessService _submissionAccessService;
    private readonly IReviewSessionRepository _sessionRepo;
    private readonly IMeetingService _meetingService;

    public LecturerService(
        IProjectGroupRepository groupRepo,
        IReviewerAssignmentRepository assignmentRepo,
        IFeedbackRepository feedbackRepo,
        IReviewRoundRepository roundRepo,
        IEvaluationRepository evaluationRepo,
        IMentorRoundReviewRepository mentorRoundReviewRepo,
        INotificationRepository notificationRepo,
        ILecturerScheduleService lecturerScheduleService,
        ILecturerWorkflowService lecturerWorkflowService,
        ISubmissionAccessService submissionAccessService,
        IReviewSessionRepository sessionRepo,
        IMeetingService meetingService)
    {
        _groupRepo = groupRepo;
        _assignmentRepo = assignmentRepo;
        _feedbackRepo = feedbackRepo;
        _roundRepo = roundRepo;
        _evaluationRepo = evaluationRepo;
        _mentorRoundReviewRepo = mentorRoundReviewRepo;
        _notificationRepo = notificationRepo;
        _lecturerScheduleService = lecturerScheduleService;
        _lecturerWorkflowService = lecturerWorkflowService;
        _submissionAccessService = submissionAccessService;
        _sessionRepo = sessionRepo;
        _meetingService = meetingService;
    }

    public async Task<LecturerDashboardDto> GetDashboardDataAsync(string lecturerId)
    {
        var groups = (await _groupRepo.GetSummariesBySupervisorAsync(lecturerId)).ToList();
        var pendingFeedbacks = (await _feedbackRepo.GetPendingApprovalDtosBySupervisorAsync(lecturerId)).ToList();
        var assignments = (await _assignmentRepo.GetAssignmentDtosByReviewerAsync(lecturerId)).ToList();
        var notifications = (await _notificationRepo.GetRecentByRecipientAsync(lecturerId, 5)).ToList();
        var today = DateTime.Today;
        var now = DateTime.Now;

        var scheduleEntries = _lecturerScheduleService.BuildEntries(groups, assignments, now);
        var deadlines = await _lecturerScheduleService.BuildDeadlinesAsync(groups, pendingFeedbacks, now);

        var recentActivities = notifications.Select(n => new DashboardActivityItemDto
            {
                Title = n.Title,
                Description = n.Content,
                Timestamp = n.CreatedAt,
                Icon = LecturerPresentationHelper.GetNotificationIcon(n.Type),
                IconBgColor = LecturerPresentationHelper.GetNotificationColor(n.Type),
                ActionUrl = LecturerPresentationHelper.ResolveNotificationUrl(n),
                ActionText = "Open"
            })
            .Concat(pendingFeedbacks.Select(f => new DashboardActivityItemDto
            {
                Title = $"{f.GroupName} feedback awaiting approval",
                Description = $"Reviewer {f.ReviewerName} submitted Round {f.RoundNumber} feedback.",
                Timestamp = f.CreatedAt,
                Icon = "fact_check",
                IconBgColor = "var(--fpt-orange)",
                ActionUrl = $"/Lecturer/FeedbackApprovalDetail/{f.FeedbackId}",
                ActionText = "Review"
            }))
            .Concat(scheduleEntries.Where(s => s.ScheduledAt >= today)
                .OrderBy(s => s.ScheduledAt)
                .Take(3)
                .Select(s => new DashboardActivityItemDto
                {
                    Title = $"{s.RoleLabel}: {s.GroupName}",
                    Description = $"{s.ProjectName} - Round {s.RoundNumber} ({s.RoundType}) at {s.ScheduledAt:MMM dd HH:mm}.",
                    Timestamp = s.ScheduledAt,
                    Icon = s.IsOnline ? "videocam" : "location_on",
                    IconBgColor = s.IsOnline ? "#0EA5E9" : "#6B7280",
                    ActionUrl = s.PrimaryActionUrl,
                    ActionText = s.PrimaryActionText
                }))
            .OrderByDescending(a => a.Timestamp)
            .Take(6)
            .ToList();

        return new LecturerDashboardDto
        {
            MentoringGroupsCount = groups.Count,
            PendingApprovalsCount = pendingFeedbacks.Count,
            AssignedReviewsCount = assignments.Count,
            UpcomingDeadlinesCount = deadlines.Count(d => d.DueAt.Date <= today.AddDays(7)),
            RecentActivities = recentActivities,
            TodaysSchedule = scheduleEntries
                .Where(s => s.ScheduledAt >= now)
                .OrderBy(s => s.ScheduledAt)
                .Take(4)
                .Select(s => new DashboardScheduleItemDto
                {
                    StartTime = s.ScheduledAt,
                    Title = $"{s.RoleLabel}: {s.GroupName}",
                    Location = s.Location,
                    DurationMinutes = 60,
                    IsHighlight = s.IsOnline || s.RoleLabel == "Reviewer",
                    ActionUrl = s.PrimaryActionUrl,
                    MeetLink = s.MeetLink
                })
                .ToList(),
            GuidanceMessage = pendingFeedbacks.Any()
                ? "You have feedback waiting for approval. Please review it before the student release window."
                : "Your dashboard combines mentoring status, reviews, deadlines, and meeting information."
        };
    }

    public async Task<LecturerProjectsDto> GetMentoredProjectsAsync(string lecturerId)
    {
        var groups = (await _groupRepo.GetSummariesBySupervisorAsync(lecturerId)).ToList();
        var dto = new LecturerProjectsDto();
        var roundsBySemester = new Dictionary<int, int>();
        var now = DateTime.Now;

        foreach (var group in groups)
        {
            var submittedEvaluations = group.Evaluations
                .Where(e => e.Status == EvaluationStatus.Submitted.ToString())
                .OrderByDescending(e => e.SubmittedAt)
                .ToList();
            var latestEvaluation = submittedEvaluations.FirstOrDefault();
            var nextSession = group.Sessions
                .Where(rs => rs.ScheduledAt >= now)
                .OrderBy(rs => rs.ScheduledAt)
                .FirstOrDefault();
            var pendingFeedbackCount = group.Evaluations
                .Count(e => e.ApprovalStatus == ApprovalStatus.Pending.ToString());
            if (!roundsBySemester.TryGetValue(group.SemesterId, out var totalRounds))
            {
                totalRounds = Math.Max(1, (await _roundRepo.GetBySemesterAsync(group.SemesterId)).Count());
                roundsBySemester[group.SemesterId] = totalRounds;
            }
            var progressPercent = Math.Min(100, submittedEvaluations.Count * 100 / totalRounds);
            var supervisorRole = group.SupervisorRoles
                .FirstOrDefault(sr => string.Equals(sr.LecturerId, lecturerId, StringComparison.OrdinalIgnoreCase))?.Role
                ?? ProjectRole.Main.ToString();

            dto.Projects.Add(new LecturerProjectItemDto
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                ProjectName = group.ProjectName,
                ProjectCode = group.ProjectCode,
                Semester = group.SemesterCode,
                SupervisorRole = supervisorRole,
                MemberNames = group.MemberNames,
                CurrentRound = latestEvaluation != null ? $"Round {latestEvaluation.RoundNumber}" : "No review submitted yet",
                Status = pendingFeedbackCount > 0
                    ? "Awaiting feedback approval"
                    : latestEvaluation?.ApprovalStatus == ApprovalStatus.Rejected.ToString()
                        ? "Needs reviewer revision"
                        : "On track",
                ProgressPercent = progressPercent,
                NextSessionAt = nextSession?.ScheduledAt,
                NextSessionLocation = nextSession == null ? "No session scheduled" : LecturerPresentationHelper.ResolveLocation(nextSession),
                PendingFeedbackCount = pendingFeedbackCount
            });
        }

        return dto;
    }

    public async Task<LecturerProjectGroupDetailDto> GetProjectGroupDetailAsync(string lecturerId, int groupId)
    {
        var group = await _groupRepo.GetByIdAsync(groupId);
        if (group == null)
        {
            throw new InvalidOperationException("Group not found.");
        }

        var project = group.Project;
        var isAuthorizedSupervisor = project.ProjectSupervisors.Any(ps => ps.LecturerID == lecturerId);
        if (!isAuthorizedSupervisor)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this group.");
        }

        var rounds = (await _roundRepo.GetBySemesterAsync(project.SemesterID))
            .OrderBy(r => r.RoundNumber)
            .ToList();
        var pendingFeedbacks = (await _feedbackRepo.GetPendingApprovalDtosBySupervisorAsync(lecturerId)).ToList();
        var nextMeeting = group.ReviewSessions
            .Where(rs => rs.ScheduledAt >= DateTime.Now)
            .OrderBy(rs => rs.ScheduledAt)
            .FirstOrDefault();
        var supervisor = project.ProjectSupervisors
            .OrderBy(ps => ps.Role == ProjectRole.Main ? 0 : 1)
            .FirstOrDefault();

        return new LecturerProjectGroupDetailDto
        {
            GroupId = group.GroupID,
            GroupName = group.GroupName,
            ProjectName = project.ProjectName,
            ProjectCode = project.ProjectCode,
            Semester = project.Semester?.SemesterCode ?? string.Empty,
            SupervisorName = supervisor?.Lecturer?.FullName ?? "N/A",
            PendingFeedbackId = pendingFeedbacks
                .Where(f => f.GroupId == groupId)
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => (int?)f.FeedbackId)
                .FirstOrDefault(),
            Members = group.GroupMembers.Select(LecturerPresentationHelper.MapToStudentMemberDto).ToList(),
            Milestones = rounds.Select(round =>
            {
                var submission = group.Submissions
                    .Where(s => s.Requirement.ReviewRoundID == round.ReviewRoundID)
                    .OrderByDescending(s => s.Version)
                    .FirstOrDefault();
                var evaluation = group.Evaluations
                    .Where(e => e.ReviewRoundID == round.ReviewRoundID)
                    .OrderByDescending(e => e.SubmittedAt)
                    .FirstOrDefault();
                var session = group.ReviewSessions
                    .Where(rs => rs.ReviewRoundID == round.ReviewRoundID)
                    .OrderBy(rs => rs.ScheduledAt)
                    .FirstOrDefault();
                var reviewerAssignment = group.ReviewerAssignments
                    .Where(ra => ra.ReviewRoundID == round.ReviewRoundID)
                    .OrderByDescending(ra => ra.AssignedAt)
                    .FirstOrDefault();
                var mentorGate = group.MentorRoundReviews
                    .FirstOrDefault(mr => mr.ReviewRoundID == round.ReviewRoundID);
                var deadline = round.SubmissionRequirements
                    .OrderBy(r => r.Deadline)
                    .Select(r => r.Deadline)
                    .FirstOrDefault();

                return new MilestoneDetailDto
                {
                    RoundId = round.ReviewRoundID,
                    RoundNumber = round.RoundNumber,
                    SubmissionId = submission?.SubmissionID,
                    Title = $"Round {round.RoundNumber}",
                    RoundType = round.RoundType.ToString(),
                    StartDate = round.StartDate,
                    EndDate = round.EndDate,
                    SubmittedAt = submission?.SubmittedAt,
                    Deadline = deadline == default ? round.EndDate : deadline,
                    Status = LecturerPresentationHelper.ResolveMilestoneStatus(round, submission, evaluation, mentorGate),
                    MentorGateStatus = mentorGate?.DecisionStatus ?? MentorGateStatus.Pending,
                    MentorGateComment = mentorGate?.ProgressComment,
                    ReportDocumentUrl = submission?.FileUrl,
                    SubmissionFileName = submission?.FileName,
                    SubmissionSizeMb = submission?.FileSizeMB,
                    SubmittedByName = submission?.Submitter?.FullName,
                    ReviewerName = reviewerAssignment?.Reviewer?.FullName ?? evaluation?.Reviewer?.FullName,
                    FeedbackStatus = evaluation?.Feedback?.FeedbackApproval?.ApprovalStatus.ToString(),
                    ScheduledAt = session?.ScheduledAt,
                    Location = session != null ? LecturerPresentationHelper.ResolveLocation(session) : null
                };
            }).ToList(),
            NextMeeting = nextMeeting == null
                ? null
                : new MeetingScheduleDto
                {
                    ScheduledAt = nextMeeting.ScheduledAt,
                    IsOnline = LecturerPresentationHelper.IsOnlineSession(nextMeeting),
                    Location = LecturerPresentationHelper.ResolveLocation(nextMeeting),
                    Title = $"Round {nextMeeting.ReviewRound?.RoundNumber ?? 0} review"
                }
        };
    }

    public async Task<LecturerFeedbackApprovalsDto> GetPendingApprovalsAsync(string lecturerId)
    {
        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalDtosBySupervisorAsync(lecturerId);
        var dto = new LecturerFeedbackApprovalsDto();
        dto.PendingFeedbacks.AddRange(pendingFeedbacks);
        return dto;
    }

    public async Task<LecturerFeedbackApprovalDetailDto> GetFeedbackApprovalDetailAsync(string lecturerId, int feedbackId)
    {
        var dto = await _feedbackRepo.GetApprovalDetailDtoAsync(feedbackId);
        if (dto == null)
        {
            throw new InvalidOperationException("Feedback not found.");
        }

        // Auth check — need entity for ProjectSupervisors navigation
        var feedback = await _feedbackRepo.GetByIdWithDetailsAsync(feedbackId);
        var isAuthorizedSupervisor =
            string.Equals(feedback!.FeedbackApproval?.SupervisorID, lecturerId, StringComparison.OrdinalIgnoreCase) ||
            feedback.Evaluation.Group.Project?.ProjectSupervisors.Any(ps => ps.LecturerID == lecturerId) == true;
        if (!isAuthorizedSupervisor)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this feedback.");
        }

        var project = feedback.Evaluation.Group.Project ?? throw new InvalidOperationException("Project not found.");
        var mentorGate = feedback.Evaluation.Group.MentorRoundReviews?.FirstOrDefault(m => m.ReviewRoundID == feedback.Evaluation.ReviewRoundID)
            ?? await _mentorRoundReviewRepo.GetAsync(feedback.Evaluation.ReviewRoundID, feedback.Evaluation.GroupID);

        dto.TotalRounds = Math.Max(1, (await _roundRepo.GetBySemesterAsync(project.SemesterID)).Count());
        dto.MentorGateStatus = mentorGate?.DecisionStatus ?? MentorGateStatus.Pending;
        dto.MentorGateComment = mentorGate?.ProgressComment;
        return dto;
    }

    public async Task<LecturerReviewAssignmentsDto> GetReviewAssignmentsAsync(string reviewerId)
    {
        var assignments = (await _assignmentRepo.GetAssignmentDtosByReviewerAsync(reviewerId)).ToList();
        var dto = new LecturerReviewAssignmentsDto();
        dto.Assignments.AddRange(assignments);
        dto.PendingEvaluationsCount = dto.Assignments.Count(a => !a.HasEvaluation);
        dto.ScheduledTodayCount = dto.Assignments.Count(a => a.ScheduledAt?.Date == DateTime.Today);
        dto.CompletedReviewsCount = dto.Assignments.Count(a => a.HasEvaluation);
        return dto;
    }

    public async Task<LecturerScheduleDto> GetScheduleAsync(string lecturerId, string? roleFilter = null, string? rangeFilter = null, int weekOffset = 0)
    {
        var groups = (await _groupRepo.GetSummariesBySupervisorAsync(lecturerId)).ToList();
        var assignments = (await _assignmentRepo.GetAssignmentDtosByReviewerAsync(lecturerId)).ToList();
        var pendingFeedbacks = (await _feedbackRepo.GetPendingApprovalDtosBySupervisorAsync(lecturerId)).ToList();
        var now = DateTime.Now;

        return await _lecturerScheduleService.BuildScheduleAsync(
            groups,
            assignments,
            pendingFeedbacks,
            roleFilter,
            rangeFilter,
            weekOffset,
            now);
    }

    public async Task<LecturerHistoryDto> GetHistoryAsync(string lecturerId)
    {
        var reviewHistory = await _evaluationRepo.GetHistoryDtosByReviewerAsync(lecturerId);
        var feedbackHistory = await _feedbackRepo.GetHistoryDtosBySupervisorAsync(lecturerId);

        return new LecturerHistoryDto
        {
            ReviewHistory = reviewHistory.ToList(),
            FeedbackHistory = feedbackHistory.ToList()
        };
    }

    public async Task<LecturerEvaluationFormDto?> GetEvaluationFormAsync(string reviewerId, int assignmentId)
    {
        return await _lecturerWorkflowService.GetEvaluationFormAsync(reviewerId, assignmentId);
    }

    public async Task<(bool Success, string ErrorMessage)> SubmitEvaluationAsync(string reviewerId, EvaluationSubmitDto model)
    {
        return await _lecturerWorkflowService.SubmitEvaluationAsync(reviewerId, model);
    }

    public async Task<(bool Success, string ErrorMessage)> ReviewRoundGateAsync(string supervisorId, int groupId, int roundId, MentorGateStatus decision, string? progressComment)
    {
        return await _lecturerWorkflowService.ReviewRoundGateAsync(supervisorId, groupId, roundId, decision, progressComment);
    }

    public async Task<bool> ApproveFeedbackAsync(string supervisorId, FeedbackApprovalDecisionDto model)
    {
        return await _lecturerWorkflowService.ApproveFeedbackAsync(supervisorId, model);
    }

    public async Task<(byte[] content, string fileName, string contentType)?> GetSubmissionFileAsync(int submissionId)
    {
        return await _submissionAccessService.GetSubmissionFileAsync(submissionId);
    }

    public async Task<bool> CanUserAccessSubmissionAsync(string userId, int submissionId, string role)
    {
        return await _submissionAccessService.CanUserAccessSubmissionAsync(userId, submissionId, role);
    }

    public async Task<(bool Success, string ErrorMessage)> ScheduleReviewMeetingAsync(string reviewerId, int assignmentId, DateTime scheduledAt)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            return (false, "Assignment not found.");
        }

        if (!string.Equals(assignment.ReviewerID, reviewerId, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "You are not authorized to schedule this meeting.");
        }

        var round = assignment.ReviewRound;
        if (round == null)
        {
            return (false, "Review round details not found.");
        }

        if (round.RoundType != RoundType.Online)
        {
            return (false, "Scheduling meetings through this system is only available for Online rounds.");
        }

        // Validate constraint: time must be within [Round.EndDate - 3 days, Round.EndDate]
        var allowedStart = round.EndDate.AddDays(-3);
        if (scheduledAt < allowedStart || scheduledAt > round.EndDate)
        {
            return (false, $"The scheduled time must be between {allowedStart:MMM dd HH:mm} and {round.EndDate:MMM dd HH:mm}.");
        }

        // Fetch or create the session
        var session = await _sessionRepo.GetByRoundAndGroupAsync(round.ReviewRoundID, assignment.GroupID);
        var isNewSession = false;
        
        if (session == null)
        {
            session = new ReviewSessionInfo
            {
                GroupID = assignment.GroupID,
                ReviewRoundID = round.ReviewRoundID,
                IsOnline = true
            };
            isNewSession = true;
        }

        // Update time
        session.ScheduledAt = scheduledAt;

        // Generate Google Meet link only if one doesn't exist
        if (string.IsNullOrEmpty(session.MeetLink))
        {
            var summary = $"Review: {assignment.Group?.Project?.ProjectName ?? "Project"} (Round {round.RoundNumber})";
            var description = $"Online review session for group {assignment.Group?.GroupName}.";
            
            // Assume 1 hour duration
            var meetLink = await _meetingService.CreateOnlineMeetingAsync(summary, description, scheduledAt, scheduledAt.AddHours(1));
            session.MeetLink = meetLink;
        }

        if (isNewSession)
        {
            await _sessionRepo.AddAsync(session);
        }
        else
        {
            await _sessionRepo.UpdateAsync(session);
        }

        await _sessionRepo.SaveChangesAsync();

        return (true, string.Empty);
    }
}
