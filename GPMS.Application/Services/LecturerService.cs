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
    private readonly IEmailService _emailService;

    public LecturerService(
        IProjectGroupRepository groupRepo,
        IReviewerAssignmentRepository assignmentRepo,
        IFeedbackRepository feedbackRepo,
        IReviewRoundRepository roundRepo,
        IEvaluationRepository evaluationRepo,
        IMentorRoundReviewRepository mentorRoundReviewRepo,
        INotificationRepository notificationRepo,
        IEmailService emailService)
    {
        _groupRepo = groupRepo;
        _assignmentRepo = assignmentRepo;
        _feedbackRepo = feedbackRepo;
        _roundRepo = roundRepo;
        _evaluationRepo = evaluationRepo;
        _mentorRoundReviewRepo = mentorRoundReviewRepo;
        _notificationRepo = notificationRepo;
        _emailService = emailService;
    }

    public async Task<LecturerDashboardDto> GetDashboardDataAsync(string lecturerId)
    {
        var groups = (await _groupRepo.GetBySupervisorAsync(lecturerId)).ToList();
        var pendingFeedbacks = (await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId)).ToList();
        var assignments = (await _assignmentRepo.GetByReviewerAsync(lecturerId)).ToList();
        var notifications = (await _notificationRepo.GetRecentByRecipientAsync(lecturerId, 5)).ToList();
        var today = DateTime.Today;
        var now = DateTime.Now;

        var scheduleEntries = BuildScheduleEntries(groups, assignments);
        var deadlines = await BuildDeadlinesAsync(groups, pendingFeedbacks);

        var recentActivities = notifications.Select(n => new DashboardActivityItemDto
            {
                Title = n.Title,
                Description = n.Content,
                Timestamp = n.CreatedAt,
                Icon = GetNotificationIcon(n.Type),
                IconBgColor = GetNotificationColor(n.Type),
                ActionUrl = ResolveNotificationUrl(n),
                ActionText = "Open"
            })
            .Concat(pendingFeedbacks.Select(f => new DashboardActivityItemDto
            {
                Title = $"{f.Evaluation?.Group?.GroupName ?? "Group"} feedback awaiting approval",
                Description = $"Reviewer {f.Evaluation?.Reviewer?.FullName ?? "N/A"} submitted Round {f.Evaluation?.ReviewRound?.RoundNumber ?? 0} feedback.",
                Timestamp = f.CreatedAt,
                Icon = "fact_check",
                IconBgColor = "var(--fpt-orange)",
                ActionUrl = $"/Lecturer/FeedbackApprovalDetail/{f.FeedbackID}",
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
                    ActionUrl = s.ActionUrl,
                    ActionText = "Open"
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
                    MeetLink = s.MeetLink,
                    ActionUrl = s.ActionUrl
                })
                .ToList(),
            GuidanceMessage = pendingFeedbacks.Any()
                ? "You have feedback waiting for approval. Please review it before the student release window."
                : "Your dashboard combines mentoring status, reviews, deadlines, and meeting information."
        };
    }

    public async Task<LecturerProjectsDto> GetMentoredProjectsAsync(string lecturerId)
    {
        var groups = (await _groupRepo.GetBySupervisorAsync(lecturerId)).ToList();
        var dto = new LecturerProjectsDto();

        foreach (var group in groups)
        {
            var project = group.Project;
            if (project == null)
            {
                continue;
            }

            var submittedEvaluations = group.Evaluations
                .Where(e => e.Status == EvaluationStatus.Submitted)
                .OrderByDescending(e => e.SubmittedAt)
                .ToList();
            var latestEvaluation = submittedEvaluations.FirstOrDefault();
            var nextSession = group.ReviewSessions
                .Where(rs => rs.ScheduledAt >= DateTime.Now)
                .OrderBy(rs => rs.ScheduledAt)
                .FirstOrDefault();
            var pendingFeedbackCount = group.Evaluations.Count(e =>
                e.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Pending);
            var totalRounds = Math.Max(1, (await _roundRepo.GetBySemesterAsync(project.SemesterID)).Count());
            var progressPercent = Math.Min(100, submittedEvaluations.Count * 100 / totalRounds);

            dto.Projects.Add(new LecturerProjectItemDto
            {
                GroupId = group.GroupID,
                GroupName = group.GroupName,
                ProjectName = project.ProjectName,
                ProjectCode = project.ProjectCode,
                Semester = project.Semester?.SemesterCode ?? string.Empty,
                SupervisorRole = project.ProjectSupervisors
                    .FirstOrDefault(ps => ps.LecturerID == lecturerId)?.Role.ToString() ?? ProjectRole.Main.ToString(),
                MemberNames = group.GroupMembers.Select(m => m.User?.FullName ?? "Unknown").ToList(),
                CurrentRound = latestEvaluation != null ? $"Round {latestEvaluation.ReviewRound?.RoundNumber ?? 0}" : "No review submitted yet",
                Status = pendingFeedbackCount > 0
                    ? "Awaiting feedback approval"
                    : latestEvaluation?.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Rejected
                        ? "Needs reviewer revision"
                        : "On track",
                ProgressPercent = progressPercent,
                NextSessionAt = nextSession?.ScheduledAt,
                NextSessionLocation = nextSession != null ? ResolveLocation(nextSession) : null,
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

        var rounds = (await _roundRepo.GetBySemesterAsync(project.SemesterID))
            .OrderBy(r => r.RoundNumber)
            .ToList();
        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId);
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
                .Where(f => f.Evaluation?.GroupID == groupId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => (int?)f.FeedbackID)
                .FirstOrDefault(),
            Members = group.GroupMembers.Select(m => new StudentMemberDto
            {
                UserId = m.UserID,
                FullName = m.User?.FullName ?? "Unknown",
                Email = m.User?.Email,
                RoleInGroup = m.RoleInGroup.ToString(),
                AvatarUrl = BuildAvatarUrl(m.User?.FullName)
            }).ToList(),
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
                    Title = $"Round {round.RoundNumber}",
                    RoundType = round.RoundType.ToString(),
                    StartDate = round.StartDate,
                    EndDate = round.EndDate,
                    SubmittedAt = submission?.SubmittedAt,
                    Deadline = deadline == default ? round.EndDate : deadline,
                    Status = ResolveMilestoneStatus(round, submission, evaluation, mentorGate),
                    MentorGateStatus = mentorGate?.DecisionStatus ?? MentorGateStatus.Pending,
                    MentorGateComment = mentorGate?.ProgressComment,
                    ReportDocumentUrl = submission?.FileUrl,
                    SubmissionFileName = submission?.FileName,
                    SubmissionSizeMb = submission?.FileSizeMB,
                    SubmittedByName = submission?.Submitter?.FullName,
                    ReviewerName = reviewerAssignment?.Reviewer?.FullName ?? evaluation?.Reviewer?.FullName,
                    FeedbackStatus = evaluation?.Feedback?.FeedbackApproval?.ApprovalStatus.ToString(),
                    ScheduledAt = session?.ScheduledAt,
                    Location = session != null ? ResolveLocation(session) : null,
                    MeetLink = session?.MeetLink
                };
            }).ToList(),
            NextMeeting = nextMeeting == null
                ? null
                : new MeetingScheduleDto
                {
                    ScheduledAt = nextMeeting.ScheduledAt,
                    IsOnline = !string.IsNullOrWhiteSpace(nextMeeting.MeetLink),
                    Location = ResolveLocation(nextMeeting),
                    MeetLink = nextMeeting.MeetLink,
                    Title = $"Round {nextMeeting.ReviewRound?.RoundNumber ?? 0} review"
                }
        };
    }

    public async Task<LecturerFeedbackApprovalsDto> GetPendingApprovalsAsync(string lecturerId)
    {
        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId);
        var dto = new LecturerFeedbackApprovalsDto();

        foreach (var feedback in pendingFeedbacks)
        {
            dto.PendingFeedbacks.Add(new PendingFeedbackItemDto
            {
                FeedbackId = feedback.FeedbackID,
                EvaluationId = feedback.EvaluationID,
                GroupName = feedback.Evaluation?.Group?.GroupName ?? "N/A",
                ProjectName = feedback.Evaluation?.Group?.Project?.ProjectName ?? "N/A",
                ReviewRoundName = feedback.Evaluation?.ReviewRound?.RoundNumber.ToString() ?? "N/A",
                RoundNumber = feedback.Evaluation?.ReviewRound?.RoundNumber ?? 0,
                ReviewerName = feedback.Evaluation?.Reviewer?.FullName ?? "N/A",
                SubmittedAt = feedback.Evaluation?.SubmittedAt ?? feedback.CreatedAt,
                AutoReleaseAt = feedback.FeedbackApproval?.ApprovedAt?.AddDays(7),
                ApprovalStatus = feedback.FeedbackApproval?.ApprovalStatus ?? ApprovalStatus.Pending
            });
        }

        return dto;
    }

    public async Task<LecturerFeedbackApprovalDetailDto> GetFeedbackApprovalDetailAsync(int feedbackId)
    {
        var feedback = await _feedbackRepo.GetByIdWithDetailsAsync(feedbackId);
        if (feedback == null)
        {
            throw new InvalidOperationException("Feedback not found.");
        }

        var evaluation = feedback.Evaluation;
        var group = evaluation.Group;
        var mentorGate = group.MentorRoundReviews?.FirstOrDefault(m => m.ReviewRoundID == evaluation.ReviewRoundID)
            ?? await _mentorRoundReviewRepo.GetAsync(evaluation.ReviewRoundID, group.GroupID);

        return new LecturerFeedbackApprovalDetailDto
        {
            FeedbackId = feedback.FeedbackID,
            EvaluationId = feedback.EvaluationID,
            GroupName = group.GroupName,
            GroupId = group.GroupID,
            ReviewRoundName = evaluation.ReviewRound.RoundNumber.ToString(),
            CurrentRoundIndex = evaluation.ReviewRound.RoundNumber,
            TotalRounds = Math.Max(1, (await _roundRepo.GetBySemesterAsync(group.Project.SemesterID)).Count()),
            SubmittedAt = evaluation.SubmittedAt ?? feedback.CreatedAt,
            ApprovalStatus = feedback.FeedbackApproval?.ApprovalStatus ?? ApprovalStatus.Pending,
            SupervisorComment = feedback.FeedbackApproval?.SupervisorComment,
            MentorGateStatus = mentorGate?.DecisionStatus ?? MentorGateStatus.Pending,
            MentorGateComment = mentorGate?.ProgressComment,
            ReviewerName = evaluation.Reviewer.FullName,
            FeedbackContent = feedback.Content,
            Scores = evaluation.EvaluationDetails
                .OrderBy(s => s.Item.OrderIndex)
                .Select(s => new EvaluationScoreItemDto
                {
                    ItemId = s.ItemID,
                    ItemCode = s.Item.ItemCode,
                    ItemName = s.Item.ItemName,
                    ItemContent = s.Item.ItemContent,
                    Section = s.Item.Section,
                    ItemType = s.Item.ItemType,
                    Assessment = s.Assessment,
                    ReviewerComment = s.Comment,
                    MentorComment = s.MentorComment,
                    GradeDescription = s.GradeDescription,
                    RubricDescriptions = s.Item.RubricDescriptions.Select(r => new RubricDescriptionDto
                    {
                        GradeLevel = r.GradeLevel,
                        Description = r.Description
                    }).ToList()
                })
                .ToList(),
            Members = group.GroupMembers.Select(m => new StudentMemberDto
            {
                UserId = m.UserID,
                FullName = m.User?.FullName ?? "Unknown",
                Email = m.User?.Email,
                RoleInGroup = m.RoleInGroup.ToString(),
                AvatarUrl = BuildAvatarUrl(m.User?.FullName)
            }).ToList()
        };
    }

    public async Task<LecturerReviewAssignmentsDto> GetReviewAssignmentsAsync(string reviewerId)
    {
        var assignments = await _assignmentRepo.GetByReviewerAsync(reviewerId);
        var dto = new LecturerReviewAssignmentsDto();

        foreach (var assignment in assignments)
        {
            var session = assignment.Group?.ReviewSessions?.FirstOrDefault(rs => rs.ReviewRoundID == assignment.ReviewRoundID);
            var evaluation = assignment.Group?.Evaluations?.FirstOrDefault(e =>
                e.ReviewRoundID == assignment.ReviewRoundID &&
                e.ReviewerID == reviewerId);
            var mentorGate = assignment.Group?.MentorRoundReviews?.FirstOrDefault(m => m.ReviewRoundID == assignment.ReviewRoundID);
            var isRejected = evaluation?.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Rejected;
            var hasCompletedEvaluation = evaluation?.Status == EvaluationStatus.Submitted && !isRejected;
            var statusNote = mentorGate?.DecisionStatus switch
            {
                MentorGateStatus.Pending => "Waiting for Mentor Approval",
                MentorGateStatus.Rejected => "Blocked by Mentor",
                _ => isRejected ? "Needs Revision" : null
            };

            dto.Assignments.Add(new ReviewAssignmentItemDto
            {
                AssignmentId = assignment.AssignmentID,
                GroupId = assignment.GroupID,
                GroupName = assignment.Group?.GroupName ?? "N/A",
                ProjectName = assignment.Group?.Project?.ProjectName ?? "N/A",
                ReviewRoundName = assignment.ReviewRound?.RoundNumber.ToString() ?? "N/A",
                RoundNumber = assignment.ReviewRound?.RoundNumber ?? 0,
                RoundType = assignment.ReviewRound?.RoundType.ToString() ?? "N/A",
                ScheduledAt = session?.ScheduledAt,
                Location = session != null ? ResolveLocation(session) : "Location pending",
                MeetLink = session?.MeetLink,
                IsOnline = !string.IsNullOrWhiteSpace(session?.MeetLink),
                HasEvaluation = hasCompletedEvaluation,
                EvaluationId = evaluation?.EvaluationID,
                StatusNote = statusNote
            });
        }

        dto.PendingEvaluationsCount = dto.Assignments.Count(a => !a.HasEvaluation);
        dto.ScheduledTodayCount = dto.Assignments.Count(a => a.ScheduledAt?.Date == DateTime.Today);
        dto.CompletedReviewsCount = dto.Assignments.Count(a => a.HasEvaluation);
        return dto;
    }

    public async Task<LecturerScheduleDto> GetScheduleAsync(string lecturerId)
    {
        var groups = (await _groupRepo.GetBySupervisorAsync(lecturerId)).ToList();
        var assignments = (await _assignmentRepo.GetByReviewerAsync(lecturerId)).ToList();
        var pendingFeedbacks = (await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId)).ToList();

        var entries = BuildScheduleEntries(groups, assignments)
            .OrderBy(s => s.ScheduledAt)
            .ToList();
        var deadlines = await BuildDeadlinesAsync(groups, pendingFeedbacks);

        return new LecturerScheduleDto
        {
            TodaySessionsCount = entries.Count(e => e.ScheduledAt.Date == DateTime.Today),
            OnlineSessionsCount = entries.Count(e => e.IsOnline),
            OfflineSessionsCount = entries.Count(e => !e.IsOnline),
            UpcomingDeadlinesCount = deadlines.Count,
            Entries = entries,
            Deadlines = deadlines
        };
    }

    public async Task<LecturerHistoryDto> GetHistoryAsync(string lecturerId)
    {
        var reviewHistory = await _evaluationRepo.GetSubmittedByReviewerAsync(lecturerId);
        var feedbackHistory = await _feedbackRepo.GetBySupervisorAsync(lecturerId);

        return new LecturerHistoryDto
        {
            ReviewHistory = reviewHistory.Select(e => new LecturerReviewHistoryItemDto
            {
                EvaluationId = e.EvaluationID,
                GroupId = e.GroupID,
                GroupName = e.Group?.GroupName ?? "N/A",
                ProjectName = e.Group?.Project?.ProjectName ?? "N/A",
                RoundNumber = e.ReviewRound?.RoundNumber ?? 0,
                RoundType = e.ReviewRound?.RoundType.ToString() ?? "N/A",
                SubmittedAt = e.SubmittedAt ?? DateTime.MinValue,
                ApprovalStatus = e.Feedback?.FeedbackApproval?.ApprovalStatus ?? ApprovalStatus.Pending,
                FeedbackPreview = Truncate(e.Feedback?.Content, 100)
            }).ToList(),
            FeedbackHistory = feedbackHistory
                .Where(f => f.FeedbackApproval != null)
                .Select(f => new LecturerFeedbackHistoryItemDto
                {
                    FeedbackId = f.FeedbackID,
                    GroupId = f.Evaluation?.GroupID ?? 0,
                    GroupName = f.Evaluation?.Group?.GroupName ?? "N/A",
                    ProjectName = f.Evaluation?.Group?.Project?.ProjectName ?? "N/A",
                    ReviewerName = f.Evaluation?.Reviewer?.FullName ?? "N/A",
                    RoundNumber = f.Evaluation?.ReviewRound?.RoundNumber ?? 0,
                    ApprovalStatus = f.FeedbackApproval?.ApprovalStatus ?? ApprovalStatus.Pending,
                    UpdatedAt = f.FeedbackApproval?.ApprovedAt ?? f.CreatedAt,
                    IsVisibleToStudent = f.FeedbackApproval?.IsVisibleToStudent ?? false,
                    SupervisorComment = f.FeedbackApproval?.SupervisorComment
                })
                .OrderByDescending(f => f.UpdatedAt)
                .ToList()
        };
    }

    public async Task<LecturerEvaluationFormDto?> GetEvaluationFormAsync(string reviewerId, int assignmentId)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
        if (assignment == null || assignment.ReviewerID != reviewerId)
        {
            return null;
        }

        var existingEvaluation = await _evaluationRepo.GetByReviewerAndGroupAsync(
            assignment.ReviewRoundID,
            reviewerId,
            assignment.GroupID);
        var session = assignment.Group?.ReviewSessions?.FirstOrDefault(rs => rs.ReviewRoundID == assignment.ReviewRoundID);
        var supervisor = assignment.Group?.Project?.ProjectSupervisors
            .OrderBy(ps => ps.Role == ProjectRole.Main ? 0 : 1)
            .FirstOrDefault();
        var mentorGate = assignment.Group?.MentorRoundReviews?.FirstOrDefault(m => m.ReviewRoundID == assignment.ReviewRoundID)
            ?? await _mentorRoundReviewRepo.GetAsync(assignment.ReviewRoundID, assignment.GroupID);
        var approvalStatus = existingEvaluation?.Feedback?.FeedbackApproval?.ApprovalStatus;
        var canEdit = existingEvaluation == null ||
                      existingEvaluation.Status == EvaluationStatus.Draft ||
                      approvalStatus == ApprovalStatus.Rejected;
        var canProceed = mentorGate?.DecisionStatus == MentorGateStatus.Approved;

        return new LecturerEvaluationFormDto
        {
            AssignmentId = assignment.AssignmentID,
            GroupId = assignment.GroupID,
            GroupName = assignment.Group?.GroupName ?? "N/A",
            ProjectName = assignment.Group?.Project?.ProjectName ?? "N/A",
            SupervisorName = supervisor?.Lecturer?.FullName ?? "N/A",
            ReviewRoundName = assignment.ReviewRound?.RoundType.ToString() ?? "N/A",
            RoundNumber = assignment.ReviewRound?.RoundNumber ?? 0,
            ScheduledAt = session?.ScheduledAt,
            Members = assignment.Group?.GroupMembers.Select(m => new StudentMemberDto
            {
                UserId = m.UserID,
                FullName = m.User?.FullName ?? "Unknown",
                Email = m.User?.Email,
                RoleInGroup = m.RoleInGroup.ToString(),
                AvatarUrl = BuildAvatarUrl(m.User?.FullName)
            }).ToList() ?? new List<StudentMemberDto>(),
            ChecklistItems = assignment.ReviewRound?.ReviewChecklist?.ChecklistItems
                .OrderBy(ci => ci.OrderIndex)
                .Select(ci => new ChecklistItemDto
                {
                    ItemId = ci.ItemID,
                    ItemCode = ci.ItemCode,
                    ItemName = ci.ItemName,
                    ItemContent = ci.ItemContent,
                    Section = ci.Section,
                    ItemType = ci.ItemType,
                    RubricDescriptions = ci.RubricDescriptions.Select(r => new RubricDescriptionDto
                    {
                        GradeLevel = r.GradeLevel,
                        Description = r.Description
                    }).ToList()
                }).ToList() ?? new List<ChecklistItemDto>(),
            ExistingEvaluationId = existingEvaluation?.EvaluationID,
            ExistingFeedbackContent = existingEvaluation?.Feedback?.Content,
            ExistingScores = existingEvaluation?.EvaluationDetails
                .OrderBy(d => d.Item.OrderIndex)
                .Select(d => new ExistingScoreDto
                {
                    ItemId = d.ItemID,
                    Assessment = d.Assessment,
                    Comment = d.Comment,
                    MentorComment = d.MentorComment,
                    GradeDescription = d.GradeDescription
                }).ToList() ?? new List<ExistingScoreDto>(),
            FeedbackApprovalStatus = approvalStatus,
            SupervisorComment = existingEvaluation?.Feedback?.FeedbackApproval?.SupervisorComment,
            MentorGateStatus = mentorGate?.DecisionStatus ?? MentorGateStatus.Pending,
            MentorGateComment = mentorGate?.ProgressComment,
            CanEdit = canEdit && canProceed
        };
    }

    public async Task<bool> SubmitEvaluationAsync(string reviewerId, EvaluationSubmitDto model)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(model.AssignmentId);
        if (assignment == null || assignment.ReviewerID != reviewerId)
        {
            return false;
        }

        var mentorGate = assignment.Group?.MentorRoundReviews?.FirstOrDefault(m => m.ReviewRoundID == assignment.ReviewRoundID)
            ?? await _mentorRoundReviewRepo.GetAsync(assignment.ReviewRoundID, assignment.GroupID);
        if (mentorGate?.DecisionStatus != MentorGateStatus.Approved)
        {
            return false;
        }

        var checklistItems = assignment.ReviewRound?.ReviewChecklist?.ChecklistItems
            .OrderBy(ci => ci.OrderIndex)
            .ToList();
        if (checklistItems == null || checklistItems.Count == 0)
        {
            return false;
        }

        var normalizedScores = new List<ScoreInputDto>();
        foreach (var item in checklistItems)
        {
            var input = model.CriteriaScores.FirstOrDefault(s => s.CriteriaId == item.ItemID);
            if (input == null)
            {
                return false;
            }

            var normalizedAssessmentValue = NormalizeAssessmentValue(item, input.Assessment);
            if (item.ItemType != "NumericScore" && string.IsNullOrWhiteSpace(normalizedAssessmentValue))
            {
                return false;
            }

            normalizedScores.Add(new ScoreInputDto
            {
                CriteriaId = item.ItemID,
                Assessment = input.Assessment,
                Comment = input.Comment?.Trim()
            });
        }

        var supervisor = assignment.Group?.Project?.ProjectSupervisors
            .OrderBy(ps => ps.Role == ProjectRole.Main ? 0 : 1)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(supervisor?.LecturerID))
        {
            return false;
        }

        var existingEvaluation = await _evaluationRepo.GetByReviewerAndGroupAsync(
            assignment.ReviewRoundID,
            reviewerId,
            assignment.GroupID);

        var now = DateTime.UtcNow;

        if (existingEvaluation == null)
        {
            var evaluation = new Evaluation
            {
                ReviewRoundID = assignment.ReviewRoundID,
                ReviewerID = reviewerId,
                GroupID = assignment.GroupID,
                Status = EvaluationStatus.Submitted,
                SubmittedAt = now,
                EvaluationDetails = normalizedScores.Select(score => new EvaluationDetail
                {
                    ItemID = score.CriteriaId,
                    Assessment = score.Assessment,
                    Comment = score.Comment,
                    GradeDescription = ResolveGradeDescription(checklistItems.First(ci => ci.ItemID == score.CriteriaId), score.Assessment)
                }).ToList(),
                Feedback = new Feedback
                {
                    Content = model.OverallFeedback.Trim(),
                    CreatedAt = now,
                    FeedbackApproval = new FeedbackApproval
                    {
                        SupervisorID = supervisor.LecturerID,
                        ApprovalStatus = ApprovalStatus.Pending,
                        IsVisibleToStudent = false
                    }
                }
            };

            await _evaluationRepo.AddAsync(evaluation);
            await _evaluationRepo.SaveChangesAsync();
            await NotifySubmitAsync(supervisor, assignment, evaluation.Feedback?.FeedbackID);
            return true;
        }

        var approvalStatus = existingEvaluation.Feedback?.FeedbackApproval?.ApprovalStatus;
        var canResubmit = existingEvaluation.Status == EvaluationStatus.Draft || approvalStatus == ApprovalStatus.Rejected;
        if (!canResubmit)
        {
            return false;
        }

        existingEvaluation.Status = EvaluationStatus.Submitted;
        existingEvaluation.SubmittedAt = now;

        foreach (var score in normalizedScores)
        {
            var detail = existingEvaluation.EvaluationDetails.FirstOrDefault(d => d.ItemID == score.CriteriaId);
            if (detail == null)
            {
                existingEvaluation.EvaluationDetails.Add(new EvaluationDetail
                {
                    EvaluationID = existingEvaluation.EvaluationID,
                    ItemID = score.CriteriaId,
                    Assessment = score.Assessment,
                    Comment = score.Comment,
                    GradeDescription = ResolveGradeDescription(checklistItems.First(ci => ci.ItemID == score.CriteriaId), score.Assessment)
                });
            }
            else
            {
                detail.Assessment = score.Assessment;
                detail.Comment = score.Comment;
                detail.GradeDescription = ResolveGradeDescription(checklistItems.First(ci => ci.ItemID == score.CriteriaId), score.Assessment);
            }
        }

        if (existingEvaluation.Feedback == null)
        {
            existingEvaluation.Feedback = new Feedback
            {
                Content = model.OverallFeedback.Trim(),
                CreatedAt = now,
                FeedbackApproval = new FeedbackApproval
                {
                    SupervisorID = supervisor.LecturerID,
                    ApprovalStatus = ApprovalStatus.Pending,
                    IsVisibleToStudent = false
                }
            };
        }
        else
        {
            existingEvaluation.Feedback.Content = model.OverallFeedback.Trim();
            existingEvaluation.Feedback.CreatedAt = now;

            if (existingEvaluation.Feedback.FeedbackApproval == null)
            {
                existingEvaluation.Feedback.FeedbackApproval = new FeedbackApproval
                {
                    SupervisorID = supervisor.LecturerID,
                    ApprovalStatus = ApprovalStatus.Pending,
                    IsVisibleToStudent = false
                };
            }
            else
            {
                existingEvaluation.Feedback.FeedbackApproval.SupervisorID = supervisor.LecturerID;
                existingEvaluation.Feedback.FeedbackApproval.ApprovalStatus = ApprovalStatus.Pending;
                existingEvaluation.Feedback.FeedbackApproval.SupervisorComment = null;
                existingEvaluation.Feedback.FeedbackApproval.ApprovedAt = null;
                existingEvaluation.Feedback.FeedbackApproval.AutoReleasedAt = null;
                existingEvaluation.Feedback.FeedbackApproval.IsVisibleToStudent = false;
            }
        }

        _evaluationRepo.Update(existingEvaluation);
        await _evaluationRepo.SaveChangesAsync();
        await NotifySubmitAsync(supervisor, assignment, existingEvaluation.Feedback?.FeedbackID);
        return true;
    }

    public async Task<bool> ReviewRoundGateAsync(string supervisorId, int groupId, int roundId, MentorGateStatus decision, string? progressComment)
    {
        var group = await _groupRepo.GetByIdAsync(groupId);
        if (group?.Project == null)
        {
            return false;
        }

        var isAuthorizedSupervisor = group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId);
        if (!isAuthorizedSupervisor)
        {
            return false;
        }

        var gate = group.MentorRoundReviews.FirstOrDefault(m => m.ReviewRoundID == roundId)
            ?? await _mentorRoundReviewRepo.GetAsync(roundId, groupId);

        var isNewGate = gate == null;
        if (isNewGate)
        {
            gate = new MentorRoundReview
            {
                ReviewRoundID = roundId,
                GroupID = groupId,
                SupervisorID = supervisorId
            };
            await _mentorRoundReviewRepo.AddAsync(gate);
        }

        gate.SupervisorID = supervisorId;
        gate.DecisionStatus = decision;
        gate.ProgressComment = progressComment?.Trim();
        gate.ReviewedAt = DateTime.UtcNow;
        gate.ReviewerNotifiedAt = decision == MentorGateStatus.Approved ? DateTime.UtcNow : null;

        if (!isNewGate)
        {
            _mentorRoundReviewRepo.Update(gate);
        }
        await _mentorRoundReviewRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveFeedbackAsync(string supervisorId, FeedbackApprovalDecisionDto model)
    {
        var feedback = await _feedbackRepo.GetByIdWithDetailsAsync(model.FeedbackId);
        if (feedback?.FeedbackApproval == null)
        {
            return false;
        }

        var isAuthorizedSupervisor =
            string.Equals(feedback.FeedbackApproval.SupervisorID, supervisorId, StringComparison.OrdinalIgnoreCase) ||
            feedback.Evaluation.Group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId);
        if (!isAuthorizedSupervisor)
        {
            return false;
        }

        var approval = feedback.FeedbackApproval;
        var evaluation = feedback.Evaluation;
        var now = DateTime.UtcNow;

        foreach (var itemComment in model.ItemComments)
        {
            var detail = evaluation.EvaluationDetails.FirstOrDefault(d => d.ItemID == itemComment.ItemId);
            if (detail != null)
            {
                detail.MentorComment = itemComment.MentorComment?.Trim();
            }
        }

        switch (model.Decision)
        {
            case "Approve":
                approval.ApprovalStatus = ApprovalStatus.Approved;
                approval.SupervisorComment = model.SupervisorComment?.Trim();
                feedback.Content = model.OverallFeedbackContent.Trim();
                approval.ApprovedAt = now;
                approval.AutoReleasedAt = now.AddDays(7);
                approval.IsVisibleToStudent = false;
                evaluation.Status = EvaluationStatus.Submitted;
                break;

            case "ApproveWithEdits":
                if (string.IsNullOrWhiteSpace(model.OverallFeedbackContent))
                {
                    return false;
                }

                feedback.Content = model.OverallFeedbackContent.Trim();
                approval.ApprovalStatus = ApprovalStatus.Approved;
                approval.SupervisorComment = model.SupervisorComment?.Trim();
                approval.ApprovedAt = now;
                approval.AutoReleasedAt = now.AddDays(7);
                approval.IsVisibleToStudent = false;
                evaluation.Status = EvaluationStatus.Submitted;
                break;

            case "Reject":
                if (string.IsNullOrWhiteSpace(model.SupervisorComment))
                {
                    return false;
                }

                approval.ApprovalStatus = ApprovalStatus.Rejected;
                approval.SupervisorComment = model.SupervisorComment.Trim();
                approval.ApprovedAt = null;
                approval.AutoReleasedAt = null;
                approval.IsVisibleToStudent = false;
                evaluation.Status = EvaluationStatus.Draft;
                break;

            default:
                return false;
        }

        _evaluationRepo.Update(evaluation);
        await _feedbackRepo.UpdateApprovalAsync(approval);
        await _feedbackRepo.SaveChangesAsync();
        await NotifyDecisionAsync(feedback, model.Decision, model.SupervisorComment ?? model.OverallFeedbackContent);
        return true;
    }

    private List<LecturerScheduleEntryDto> BuildScheduleEntries(
        IEnumerable<ProjectGroup> groups,
        IEnumerable<ReviewerAssignment> assignments)
    {
        var mentorEntries = groups.SelectMany(group => group.ReviewSessions.Select(session => new LecturerScheduleEntryDto
        {
            RoleLabel = "Mentor",
            GroupId = group.GroupID,
            GroupName = group.GroupName,
            ProjectName = group.Project?.ProjectName ?? "N/A",
            RoundNumber = session.ReviewRound?.RoundNumber ?? 0,
            RoundType = session.ReviewRound?.RoundType.ToString() ?? "N/A",
            ScheduledAt = session.ScheduledAt,
            IsOnline = !string.IsNullOrWhiteSpace(session.MeetLink),
            Location = ResolveLocation(session),
            MeetLink = session.MeetLink,
            Guidance = "Join on time and review the team's readiness before approving reviewer feedback.",
            ActionUrl = $"/Lecturer/ProjectGroupDetail/{group.GroupID}"
        }));

        var reviewerEntries = assignments
            .Select(assignment =>
            {
                var session = assignment.Group?.ReviewSessions?.FirstOrDefault(rs => rs.ReviewRoundID == assignment.ReviewRoundID);
                if (session == null)
                {
                    return null;
                }

                return new LecturerScheduleEntryDto
                {
                    RoleLabel = "Reviewer",
                    GroupId = assignment.GroupID,
                    GroupName = assignment.Group?.GroupName ?? "N/A",
                    ProjectName = assignment.Group?.Project?.ProjectName ?? "N/A",
                    RoundNumber = assignment.ReviewRound?.RoundNumber ?? 0,
                    RoundType = assignment.ReviewRound?.RoundType.ToString() ?? "N/A",
                    ScheduledAt = session.ScheduledAt,
                    IsOnline = !string.IsNullOrWhiteSpace(session.MeetLink),
                    Location = ResolveLocation(session),
                    MeetLink = session.MeetLink,
                    Guidance = "Prepare rubric notes and submit the evaluation after this review session.",
                    ActionUrl = $"/Lecturer/EvaluationForm/{assignment.AssignmentID}"
                };
            })
            .Where(entry => entry != null)!
            .Select(entry => entry!);

        return mentorEntries.Concat(reviewerEntries).OrderBy(e => e.ScheduledAt).ToList();
    }

    private async Task<List<LecturerDeadlineDto>> BuildDeadlinesAsync(
        IEnumerable<ProjectGroup> groups,
        IEnumerable<Feedback> pendingFeedbacks)
    {
        var deadlines = new List<LecturerDeadlineDto>();
        var today = DateTime.Today;

        foreach (var feedback in pendingFeedbacks)
        {
            deadlines.Add(new LecturerDeadlineDto
            {
                Title = $"Approve feedback for {feedback.Evaluation?.Group?.GroupName ?? "group"}",
                Description = $"Round {feedback.Evaluation?.ReviewRound?.RoundNumber ?? 0} feedback is waiting for your decision.",
                DueAt = feedback.CreatedAt.AddDays(2),
                Severity = "warning",
                ActionUrl = $"/Lecturer/FeedbackApprovalDetail/{feedback.FeedbackID}",
                ActionText = "Review feedback"
            });
        }

        foreach (var semesterId in groups.Select(g => g.Project.SemesterID).Distinct())
        {
            var rounds = await _roundRepo.GetBySemesterAsync(semesterId);
            foreach (var round in rounds)
            {
                foreach (var requirement in round.SubmissionRequirements.Where(r => r.Deadline.Date >= today))
                {
                    deadlines.Add(new LecturerDeadlineDto
                    {
                        Title = $"{requirement.DocumentName} deadline",
                        Description = $"Round {round.RoundNumber} {round.RoundType} submission deadline for supervised groups.",
                        DueAt = requirement.Deadline,
                        Severity = requirement.Deadline.Date <= today.AddDays(3) ? "danger" : "info",
                        ActionUrl = "/Lecturer/Schedule",
                        ActionText = "Open schedule"
                    });
                }
            }
        }

        return deadlines.OrderBy(d => d.DueAt).Take(8).ToList();
    }

    private async Task NotifySubmitAsync(ProjectSupervisor supervisor, ReviewerAssignment assignment, int? feedbackId)
    {
        var title = $"New feedback needs approval for {assignment.Group?.GroupName ?? "group"}";
        var content = $"A review for {assignment.Group?.Project?.ProjectName ?? "project"} has been submitted and is waiting for your approval.";
        var emailBody =
            $"<p>Hello {supervisor.Lecturer?.FullName ?? "Lecturer"},</p>" +
            $"<p>A reviewer has submitted evaluation feedback for <strong>{assignment.Group?.GroupName ?? "your group"}</strong> in project <strong>{assignment.Group?.Project?.ProjectName ?? "N/A"}</strong>.</p>" +
            "<p>Please open the lecturer portal to review and approve the feedback.</p>";

        await CreateNotificationAsync(
            supervisor.LecturerID,
            title,
            content,
            NotificationType.Feedback,
            "Feedback",
            feedbackId,
            supervisor.Lecturer?.Email,
            $"[GPMS] Feedback approval required - {assignment.Group?.GroupName ?? "Group"}",
            emailBody);
    }

    private async Task NotifyDecisionAsync(Feedback feedback, string decision, string comments)
    {
        var reviewer = feedback.Evaluation?.Reviewer;
        if (reviewer == null)
        {
            return;
        }

        var decisionLabel = decision switch
        {
            "Approve" => "approved",
            "ApproveWithEdits" => "approved with edits",
            "Reject" => "rejected",
            _ => "updated"
        };

        var content = decision == "Reject"
            ? $"Your feedback for {feedback.Evaluation?.Group?.Project?.ProjectName ?? "project"} was rejected. Please revise it and submit again."
            : $"Your feedback for {feedback.Evaluation?.Group?.Project?.ProjectName ?? "project"} was {decisionLabel}.";
        var emailBody =
            $"<p>Hello {reviewer.FullName},</p>" +
            $"<p>Your feedback for <strong>{feedback.Evaluation?.Group?.GroupName ?? "group"}</strong> was <strong>{decisionLabel}</strong>.</p>" +
            (string.IsNullOrWhiteSpace(comments) ? string.Empty : $"<p>Supervisor note: {comments}</p>");

        await CreateNotificationAsync(
            reviewer.UserID,
            $"Feedback {decisionLabel} for {feedback.Evaluation?.Group?.GroupName ?? "group"}",
            content,
            NotificationType.Feedback,
            "Feedback",
            feedback.FeedbackID,
            reviewer.Email,
            $"[GPMS] Feedback {decisionLabel} - {feedback.Evaluation?.Group?.GroupName ?? "Group"}",
            emailBody);
    }

    private async Task CreateNotificationAsync(
        string recipientId,
        string title,
        string content,
        NotificationType type,
        string relatedEntityType,
        int? relatedEntityId,
        string? email,
        string emailSubject,
        string emailBody)
    {
        var emailSent = await TrySendEmailAsync(email, emailSubject, emailBody);
        await _notificationRepo.AddAsync(new Notification
        {
            RecipientID = recipientId,
            Title = title,
            Content = content,
            Type = type,
            RelatedEntityType = relatedEntityType,
            RelatedEntityID = relatedEntityId,
            IsEmailSent = emailSent,
            CreatedAt = DateTime.UtcNow
        });
        await _notificationRepo.SaveChangesAsync();
    }

    private async Task<bool> TrySendEmailAsync(string? toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return false;
        }

        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
            return true;
        }
        catch
        {
            return false;
        }
    }



    private static string? NormalizeAssessmentValue(ChecklistItem item, string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return string.Empty;
        }

        return rawValue.Trim();
    }

    private static string? ResolveGradeDescription(ChecklistItem item, string? assessmentValue)
    {
        return item.RubricDescriptions.FirstOrDefault(r => r.GradeLevel == assessmentValue)?.Description;
    }

    private static string ResolveMilestoneStatus(ReviewRound round, Submission? submission, Evaluation? evaluation, MentorRoundReview? mentorGate)
    {
        if (mentorGate?.DecisionStatus == MentorGateStatus.Rejected)
        {
            return "Blocked by mentor";
        }

        if (evaluation?.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Rejected)
        {
            return "Needs reviewer revision";
        }

        if (evaluation?.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Pending)
        {
            return "Waiting for mentor approval";
        }

        if (evaluation?.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Approved)
        {
            return "Approved for student release";
        }

        if (evaluation?.Status == EvaluationStatus.Submitted)
        {
            return "Reviewed";
        }

        if (submission != null)
        {
            return mentorGate?.DecisionStatus == MentorGateStatus.Approved
                ? "Approved for reviewer evaluation"
                : "Waiting for mentor approval";
        }

        if (round.StartDate > DateTime.UtcNow)
        {
            return "Upcoming";
        }

        return "In progress";
    }

    private static string ResolveLocation(ReviewSessionInfo session)
    {
        if (!string.IsNullOrWhiteSpace(session.MeetLink))
        {
            return "Online meeting";
        }

        if (session.Room == null)
        {
            return "Offline location pending";
        }

        return string.IsNullOrWhiteSpace(session.Room.Building)
            ? session.Room.RoomCode
            : $"{session.Room.RoomCode} - {session.Room.Building}";
    }

    private static string BuildAvatarUrl(string? fullName)
    {
        return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(fullName ?? "U")}&background=E5E7EB&color=374151";
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= maxLength ? value : $"{value[..maxLength].TrimEnd()}...";
    }

    private static string GetNotificationIcon(NotificationType type) => type switch
    {
        NotificationType.Feedback => "chat",
        NotificationType.Schedule => "calendar_today",
        NotificationType.Deadline => "event_busy",
        NotificationType.Review => "assignment",
        _ => "notifications"
    };

    private static string GetNotificationColor(NotificationType type) => type switch
    {
        NotificationType.Feedback => "var(--fpt-orange)",
        NotificationType.Schedule => "#0EA5E9",
        NotificationType.Deadline => "#DC2626",
        NotificationType.Review => "#6B7280",
        _ => "#6B7280"
    };

    private static string? ResolveNotificationUrl(Notification notification)
    {
        return notification.RelatedEntityType switch
        {
            "Feedback" when notification.RelatedEntityID.HasValue => $"/Lecturer/FeedbackApprovalDetail/{notification.RelatedEntityID.Value}",
            _ => "/Lecturer/History"
        };
    }
}
