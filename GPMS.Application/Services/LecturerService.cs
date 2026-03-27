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
    private readonly IEmailService _emailService;
    private readonly ISubmissionRepository _submissionRepo;
    private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;

    public LecturerService(
        IProjectGroupRepository groupRepo,
        IReviewerAssignmentRepository assignmentRepo,
        IFeedbackRepository feedbackRepo,
        IReviewRoundRepository roundRepo,
        IEvaluationRepository evaluationRepo,
        IMentorRoundReviewRepository mentorRoundReviewRepo,
        INotificationRepository notificationRepo,
        IEmailService emailService,
        ISubmissionRepository submissionRepo,
        System.Net.Http.IHttpClientFactory httpClientFactory)
    {
        _groupRepo = groupRepo;
        _assignmentRepo = assignmentRepo;
        _feedbackRepo = feedbackRepo;
        _roundRepo = roundRepo;
        _evaluationRepo = evaluationRepo;
        _mentorRoundReviewRepo = mentorRoundReviewRepo;
        _notificationRepo = notificationRepo;
        _emailService = emailService;
        _submissionRepo = submissionRepo;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<LecturerDashboardDto> GetDashboardDataAsync(string lecturerId)
    {
        var groups = (await _groupRepo.GetSummariesBySupervisorAsync(lecturerId)).ToList();
        var pendingFeedbacks = (await _feedbackRepo.GetPendingApprovalDtosBySupervisorAsync(lecturerId)).ToList();
        var assignments = (await _assignmentRepo.GetAssignmentDtosByReviewerAsync(lecturerId)).ToList();
        var notifications = (await _notificationRepo.GetRecentByRecipientAsync(lecturerId, 5)).ToList();
        var today = DateTime.Today;
        var now = DateTime.Now;

        var scheduleEntries = BuildScheduleEntries(groups, assignments, pendingFeedbacks, now);
        var deadlines = await BuildDeadlinesAsync(groups, pendingFeedbacks, now);

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
                    MeetLink = s.MeetLink,
                    ActionUrl = s.PrimaryActionUrl
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

        foreach (var group in groups)
        {
            var submittedEvaluations = group.Evaluations
                .Where(e => e.Status == EvaluationStatus.Submitted.ToString())
                .OrderByDescending(e => e.SubmittedAt)
                .ToList();
            var latestEvaluation = submittedEvaluations.FirstOrDefault();
            var nextSession = group.Sessions
                .Where(rs => rs.ScheduledAt >= DateTime.Now)
                .OrderBy(rs => rs.ScheduledAt)
                .FirstOrDefault();
            var pendingFeedbackCount = group.Evaluations
                .Count(e => e.ApprovalStatus == ApprovalStatus.Pending.ToString());
            var totalRounds = Math.Max(1, (await _roundRepo.GetBySemesterAsync(group.SemesterId)).Count());
            var progressPercent = Math.Min(100, submittedEvaluations.Count * 100 / totalRounds);
            var supervisorRole = group.SupervisorRoles
                .FirstOrDefault(sr => string.Equals(sr.LecturerId, lecturerId, StringComparison.OrdinalIgnoreCase))?.Role
                ?? ProjectRole.Main.ToString();

            string? nextSessionLocation = null;
            if (nextSession != null)
            {
                nextSessionLocation = !string.IsNullOrWhiteSpace(nextSession.MeetLink)
                    ? "Online meeting"
                    : nextSession.RoomCode != null
                        ? (!string.IsNullOrWhiteSpace(nextSession.Building)
                            ? $"{nextSession.RoomCode} - {nextSession.Building}"
                            : nextSession.RoomCode)
                        : "Offline location pending";
            }

            dto.Projects.Add(new LecturerProjectItemDto
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                ProjectName = group.ProjectName,
                ProjectCode = group.ProjectCode,
                Semester = group.SemesterCode,
                SupervisorRole = supervisorRole,
                MemberNames = group.MemberNames,
                CurrentRound = latestEvaluation != null ? $"Round {latestEvaluation.ReviewRoundId}" : "No review submitted yet",
                Status = pendingFeedbackCount > 0
                    ? "Awaiting feedback approval"
                    : latestEvaluation?.ApprovalStatus == ApprovalStatus.Rejected.ToString()
                        ? "Needs reviewer revision"
                        : "On track",
                ProgressPercent = progressPercent,
                NextSessionAt = nextSession?.ScheduledAt,
                NextSessionLocation = nextSessionLocation,
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
                    SubmissionId = submission?.SubmissionID,
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

        roleFilter = NormalizeRoleFilter(roleFilter);
        rangeFilter = NormalizeRangeFilter(rangeFilter);

        var allEntries = BuildScheduleEntries(groups, assignments, pendingFeedbacks, now)
            .OrderByDescending(s => s.NeedsAttention)
            .ThenBy(s => s.ScheduledAt)
            .ToList();
        var roleScopedEntries = ApplyRoleFilter(allEntries, roleFilter).ToList();
        var filteredEntries = ApplyRangeFilter(roleScopedEntries, rangeFilter, now)
            .OrderByDescending(s => s.NeedsAttention)
            .ThenBy(s => s.ScheduledAt)
            .ToList();
        var deadlines = roleFilter == "reviewer"
            ? new List<LecturerDeadlineDto>()
            : await BuildDeadlinesAsync(groups, pendingFeedbacks, now);
        var weekStart = GetStartOfWeek(now.Date, DayOfWeek.Monday).AddDays(weekOffset * 7);
        var weekEnd = weekStart.AddDays(6);
        var weekEntries = roleScopedEntries
            .Where(e => e.ScheduledAt.Date >= weekStart && e.ScheduledAt.Date <= weekEnd)
            .OrderBy(e => e.ScheduledAt)
            .ToList();

        return new LecturerScheduleDto
        {
            TodaySessionsCount = roleScopedEntries.Count(e => e.IsToday),
            OnlineSessionsCount = roleScopedEntries.Count(e => e.IsOnline),
            OfflineSessionsCount = roleScopedEntries.Count(e => !e.IsOnline),
            UpcomingDeadlinesCount = deadlines.Count,
            NeedsAttentionCount = roleScopedEntries.Count(e => e.NeedsAttention),
            WeekSessionsCount = weekEntries.Count,
            ActiveRoleFilter = roleFilter,
            ActiveRangeFilter = rangeFilter,
            WeekOffset = weekOffset,
            WeekLabel = $"{weekStart:dd MMM} - {weekEnd:dd MMM}",
            WeekStartDate = weekStart,
            WeekEndDate = weekEnd,
            FocusCard = BuildFocusCard(filteredEntries, deadlines, now),
            Entries = filteredEntries,
            DayGroups = BuildDayGroups(filteredEntries, now),
            WeekDays = BuildWeekDays(weekEntries, weekStart, now),
            Deadlines = deadlines
        };
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

        var submission = assignment.Group?.Submissions
            .Where(s => s.Requirement?.ReviewRoundID == assignment.ReviewRoundID)
            .OrderByDescending(s => s.Version)
            .FirstOrDefault();

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
            SubmissionFileName = submission?.FileName,
            SubmissionUrl = submission?.FileUrl,
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

    public async Task<(bool Success, string ErrorMessage)> SubmitEvaluationAsync(string reviewerId, EvaluationSubmitDto model)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(model.AssignmentId);
        if (assignment == null || assignment.ReviewerID != reviewerId)
        {
            return (false, "Assignment not found or unauthorized.");
        }

        var mentorGate = assignment.Group?.MentorRoundReviews?.FirstOrDefault(m => m.ReviewRoundID == assignment.ReviewRoundID)
            ?? await _mentorRoundReviewRepo.GetAsync(assignment.ReviewRoundID, assignment.GroupID);
        if (mentorGate?.DecisionStatus != MentorGateStatus.Approved)
        {
            return (false, "Mentor has not approved this round.");
        }

        var checklistItems = assignment.ReviewRound?.ReviewChecklist?.ChecklistItems
            .OrderBy(ci => ci.OrderIndex)
            .ToList();
        if (checklistItems == null || checklistItems.Count == 0)
        {
            return (false, "No checklist items found for this round.");
        }

        var normalizedScores = new List<ScoreInputDto>();
        foreach (var item in checklistItems)
        {
            var input = model.CriteriaScores.FirstOrDefault(s => s.CriteriaId == item.ItemID);
            if (input == null)
            {
                return (false, $"Missing score input for criteria ID {item.ItemID}. Total submitted scores count: {model.CriteriaScores.Count}.");
            }

            var normalizedAssessmentValue = NormalizeAssessmentValue(item, input.Assessment);
            if (item.ItemType != "NumericScore" && string.IsNullOrWhiteSpace(normalizedAssessmentValue))
            {
                return (false, $"Assessment is required for {item.ItemName}.");
            }

            normalizedScores.Add(new ScoreInputDto
            {
                CriteriaId = item.ItemID,
                Assessment = normalizedAssessmentValue,
                Comment = input.Comment?.Trim()
            });
        }

        var supervisor = assignment.Group?.Project?.ProjectSupervisors
            .OrderBy(ps => ps.Role == ProjectRole.Main ? 0 : 1)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(supervisor?.LecturerID))
        {
            return (false, "Project supervisor not found.");
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
            return (true, string.Empty);
        }

        var approvalStatus = existingEvaluation.Feedback?.FeedbackApproval?.ApprovalStatus;
        var canResubmit = existingEvaluation.Status == EvaluationStatus.Draft || approvalStatus == ApprovalStatus.Rejected;
        if (!canResubmit)
        {
            if (approvalStatus == ApprovalStatus.Pending) return (false, "Feedback is currently pending approval by supervisor.");
            if (approvalStatus == ApprovalStatus.Approved) return (false, "Feedback is already approved and cannot be modified.");
            return (false, "This evaluation cannot be modified in its current state.");
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
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> ReviewRoundGateAsync(string supervisorId, int groupId, int roundId, MentorGateStatus decision, string? progressComment)
    {
        var group = await _groupRepo.GetByIdAsync(groupId);
        if (group?.Project == null)
        {
            return (false, "Project group not found.");
        }

        var isAuthorizedSupervisor = group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId);
        if (!isAuthorizedSupervisor)
        {
            return (false, "You are not allowed to update this mentor gate.");
        }

        var reviewerHasStarted = group.Evaluations.Any(e => e.ReviewRoundID == roundId);
        if (reviewerHasStarted)
        {
            return (false, "Mentor gate is locked because reviewer evaluation has already started for this round.");
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

        var gateRecord = gate ?? throw new InvalidOperationException("Unable to create mentor gate.");
        gateRecord.SupervisorID = supervisorId;
        gateRecord.DecisionStatus = decision;
        gateRecord.ProgressComment = progressComment?.Trim();
        gateRecord.ReviewedAt = DateTime.UtcNow;
        gateRecord.ReviewerNotifiedAt = decision == MentorGateStatus.Approved ? DateTime.UtcNow : null;

        if (!isNewGate)
        {
            _mentorRoundReviewRepo.Update(gateRecord);
        }
        await _mentorRoundReviewRepo.SaveChangesAsync();
        return (true, string.Empty);
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
            feedback.Evaluation.Group.Project?.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId) == true;
        if (!isAuthorizedSupervisor)
        {
            return false;
        }

        var approval = feedback.FeedbackApproval;
        if (approval.ApprovalStatus != ApprovalStatus.Pending)
        {
            return false;
        }

        var evaluation = feedback.Evaluation;
        var now = DateTime.UtcNow;

        switch (model.Decision)
        {
            case "Approve":
                approval.ApprovalStatus = ApprovalStatus.Approved;
                approval.SupervisorComment = model.SupervisorComment?.Trim();
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

                ApplyMentorChecklistComments(evaluation, model.ItemComments);
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

                ApplyMentorChecklistComments(evaluation, model.ItemComments);
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
        IEnumerable<ProjectGroupSummaryDto> groups,
        IEnumerable<ReviewAssignmentItemDto> assignments,
        IEnumerable<PendingFeedbackItemDto> pendingFeedbacks,
        DateTime now)
    {
        var mentorEntries = groups.SelectMany(group => group.Sessions.Select(session =>
        {
            var isOnline = !string.IsNullOrWhiteSpace(session.MeetLink);
            var location = isOnline ? "Online meeting" : (
                !string.IsNullOrWhiteSpace(session.RoomCode) ? 
                    (!string.IsNullOrWhiteSpace(session.Building) ? $"{session.RoomCode} - {session.Building}" : session.RoomCode) 
                    : "Location pending"
            );

            var entry = new LecturerScheduleEntryDto
            {
                RoleKey = "mentor",
                RoleLabel = "Mentor",
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                ProjectName = group.ProjectName ?? "N/A",
                RoundNumber = session.RoundNumber,
                RoundType = session.RoundType ?? "N/A",
                ScheduledAt = session.ScheduledAt,
                IsOnline = isOnline,
                Location = location,
                MeetLink = session.MeetLink,
                Guidance = "Keep the group aligned before and after the review session.",
                IsToday = session.ScheduledAt.Date == now.Date,
                IsPast = session.ScheduledAt < now,
                TimeHint = BuildTimeHint(session.ScheduledAt, now),
                PrimaryActionText = "Open group",
                PrimaryActionUrl = $"/Lecturer/ProjectGroupDetail/{group.GroupId}"
            };

            var pendingEvaluation = group.Evaluations
                .Where(e => e.ReviewRoundId == session.ReviewRoundId && e.ApprovalStatus == ApprovalStatus.Pending.ToString())
                .OrderByDescending(e => e.SubmittedAt)
                .FirstOrDefault();

            var isLive = IsLiveSession(session.ScheduledAt, now);

            if (pendingEvaluation != null && pendingEvaluation.FeedbackId.HasValue)
            {
                entry.StatusKey = "needs-approval";
                entry.StatusLabel = "Need approval";
                entry.StatusTone = "danger";
                entry.NeedsAttention = true;
                entry.Guidance = "Reviewer feedback is waiting for your decision before it is released.";
                entry.PrimaryActionText = "Review feedback";
                entry.PrimaryActionUrl = $"/Lecturer/FeedbackApprovalDetail/{pendingEvaluation.FeedbackId.Value}";
                entry.SecondaryActionText = "Open group";
                entry.SecondaryActionUrl = $"/Lecturer/ProjectGroupDetail/{group.GroupId}";
            }
            else if (isLive)
            {
                entry.StatusKey = "live";
                entry.StatusLabel = "Live now";
                entry.StatusTone = "warning";
                entry.Guidance = "Stay with the team during the session and capture any follow-up actions.";
                if (!string.IsNullOrWhiteSpace(entry.MeetLink))
                {
                    entry.PrimaryActionText = "Join Meet";
                    entry.PrimaryActionUrl = entry.MeetLink;
                    entry.SecondaryActionText = "Open group";
                    entry.SecondaryActionUrl = $"/Lecturer/ProjectGroupDetail/{group.GroupId}";
                }
            }
            else if (session.ScheduledAt < now)
            {
                entry.StatusKey = "completed";
                entry.StatusLabel = "Completed";
                entry.StatusTone = "success";
                entry.Guidance = "Review notes and check whether the next round requires your approval.";
            }
            else if (session.ScheduledAt.Date == now.Date)
            {
                entry.StatusKey = "today";
                entry.StatusLabel = "Today";
                entry.StatusTone = "info";
                entry.Guidance = "Review progress notes and be ready to coach the team before the session starts.";
                if (!string.IsNullOrWhiteSpace(entry.MeetLink))
                {
                    entry.PrimaryActionText = "Join Meet";
                    entry.PrimaryActionUrl = entry.MeetLink;
                    entry.SecondaryActionText = "Open group";
                    entry.SecondaryActionUrl = $"/Lecturer/ProjectGroupDetail/{group.GroupId}";
                }
            }
            else
            {
                entry.StatusKey = "upcoming";
                entry.StatusLabel = "Upcoming";
                entry.StatusTone = "info";
            }

            return entry;
        }));

        var reviewerEntries = assignments
            .Select(assignment =>
            {
                if (!assignment.ScheduledAt.HasValue)
                {
                    return null;
                }

                bool hasSubmitted = assignment.HasEvaluation;
                bool needsRevision = assignment.StatusNote == "Needs Revision";
                bool waitingMentor = assignment.StatusNote == "Waiting for Mentor Approval" || 
                                     (hasSubmitted && !needsRevision && assignment.StatusNote == null);
                bool needsEvaluation = assignment.ScheduledAt.Value < now && (!hasSubmitted || needsRevision);
                bool isLive = IsLiveSession(assignment.ScheduledAt.Value, now);

                return new LecturerScheduleEntryDto
                {
                    RoleKey = "reviewer",
                    RoleLabel = "Reviewer",
                    GroupId = assignment.GroupId,
                    GroupName = assignment.GroupName ?? "N/A",
                    ProjectName = assignment.ProjectName ?? "N/A",
                    RoundNumber = assignment.RoundNumber,
                    RoundType = assignment.RoundType ?? "N/A",
                    ScheduledAt = assignment.ScheduledAt.Value,
                    IsOnline = assignment.IsOnline,
                    Location = assignment.Location,
                    MeetLink = assignment.MeetLink,
                    Guidance = needsRevision
                        ? "Update the evaluation with the mentor's requested changes."
                        : needsEvaluation
                            ? "The session has passed. Please complete and submit the evaluation."
                            : waitingMentor
                                ? "Your evaluation is submitted and waiting for mentor approval."
                                : "Prepare rubric notes before the session and submit the evaluation afterward.",
                    StatusKey = needsRevision
                        ? "needs-revision"
                        : needsEvaluation
                            ? "needs-evaluation"
                            : waitingMentor
                                ? "waiting-mentor"
                                : hasSubmitted
                                    ? "completed"
                                    : isLive
                                        ? "live"
                                        : assignment.ScheduledAt.Value.Date == now.Date
                                            ? "today"
                                            : "upcoming",
                    StatusLabel = needsRevision
                        ? "Needs revision"
                        : needsEvaluation
                            ? "Need evaluation"
                            : waitingMentor
                                ? "Waiting mentor approval"
                                : hasSubmitted
                                    ? "Completed"
                                    : isLive
                                        ? "Live now"
                                        : assignment.ScheduledAt.Value.Date == now.Date
                                            ? "Today"
                                            : "Upcoming",
                    StatusTone = needsRevision || needsEvaluation
                        ? "danger"
                        : waitingMentor || isLive
                            ? "warning"
                            : hasSubmitted
                                ? "success"
                                : "info",
                    NeedsAttention = needsRevision || needsEvaluation,
                    IsToday = assignment.ScheduledAt.Value.Date == now.Date,
                    IsPast = assignment.ScheduledAt.Value < now,
                    TimeHint = BuildTimeHint(assignment.ScheduledAt.Value, now),
                    PrimaryActionText = needsRevision
                        ? "Revise evaluation"
                        : needsEvaluation
                            ? "Open evaluation"
                            : !string.IsNullOrWhiteSpace(assignment.MeetLink) && !hasSubmitted && assignment.ScheduledAt.Value >= now
                                ? "Join Meet"
                                : "Open evaluation",
                    PrimaryActionUrl = !string.IsNullOrWhiteSpace(assignment.MeetLink) && !hasSubmitted && assignment.ScheduledAt.Value >= now
                        ? assignment.MeetLink!
                        : $"/Lecturer/EvaluationForm/{assignment.AssignmentId}",
                    SecondaryActionText = !string.IsNullOrWhiteSpace(assignment.MeetLink) && (!needsRevision && !needsEvaluation) && (assignment.ScheduledAt.Value >= now)
                        ? "Open evaluation"
                        : null,
                    SecondaryActionUrl = !string.IsNullOrWhiteSpace(assignment.MeetLink) && (!needsRevision && !needsEvaluation) && (assignment.ScheduledAt.Value >= now)
                        ? $"/Lecturer/EvaluationForm/{assignment.AssignmentId}"
                        : null
                };
            })
            .Where(entry => entry != null)!
            .Select(entry => entry!);

        return mentorEntries.Concat(reviewerEntries).OrderBy(e => e.ScheduledAt).ToList();
    }

    private async Task<List<LecturerDeadlineDto>> BuildDeadlinesAsync(
        IEnumerable<ProjectGroupSummaryDto> groups,
        IEnumerable<PendingFeedbackItemDto> pendingFeedbacks,
        DateTime now)
    {
        var deadlines = new List<LecturerDeadlineDto>();
        var today = now.Date;

        foreach (var feedback in pendingFeedbacks)
        {
            var dueAt = feedback.CreatedAt.AddDays(2);
            deadlines.Add(new LecturerDeadlineDto
            {
                Title = $"Approve feedback for {feedback.GroupName}",
                Description = $"Round {feedback.RoundNumber} feedback is waiting for your decision.",
                DueAt = dueAt,
                Severity = dueAt.Date <= today.AddDays(1) ? "danger" : "warning",
                ActionUrl = $"/Lecturer/FeedbackApprovalDetail/{feedback.FeedbackId}",
                ActionText = "Review feedback"
            });
        }

        var roundsBySemester = new Dictionary<int, IReadOnlyCollection<ReviewRound>>();

        foreach (var group in groups)
        {
            var projectId = group.ProjectId;
            if (!roundsBySemester.TryGetValue(group.SemesterId, out var rounds))
            {
                rounds = (await _roundRepo.GetBySemesterAsync(group.SemesterId)).ToList();
                roundsBySemester[group.SemesterId] = rounds;
            }

            foreach (var round in rounds)
            {
                foreach (var requirement in round.SubmissionRequirements.Where(r => r.Deadline.Date >= today))
                {
                    var hasSubmission = group.Submissions.Any(s => s.ReviewRoundId == round.ReviewRoundID);
                    if (hasSubmission)
                    {
                        continue;
                    }

                    deadlines.Add(new LecturerDeadlineDto
                    {
                        Title = $"{group.GroupName}: {requirement.DocumentName}",
                        Description = $"Round {round.RoundNumber} {round.RoundType} submission is still missing for this supervised group.",
                        DueAt = requirement.Deadline,
                        Severity = requirement.Deadline.Date <= today.AddDays(3) ? "danger" : "info",
                        ActionUrl = $"/Lecturer/ProjectGroupDetail/{group.GroupId}",
                        ActionText = "Open group"
                    });
                }
            }
        }

        return deadlines
            .OrderBy(d => d.DueAt)
            .ThenByDescending(d => d.Severity == "danger")
            .Take(8)
            .ToList();
    }

    private static string NormalizeRoleFilter(string? roleFilter)
    {
        return roleFilter?.Trim().ToLowerInvariant() switch
        {
            "mentor" => "mentor",
            "reviewer" => "reviewer",
            _ => "all"
        };
    }

    private static string NormalizeRangeFilter(string? rangeFilter)
    {
        return rangeFilter?.Trim().ToLowerInvariant() switch
        {
            "today" => "today",
            "upcoming" => "upcoming",
            "past" => "past",
            "all" => "all",
            _ => "week"
        };
    }

    private static IEnumerable<LecturerScheduleEntryDto> ApplyRoleFilter(
        IEnumerable<LecturerScheduleEntryDto> entries,
        string roleFilter)
    {
        return roleFilter switch
        {
            "mentor" => entries.Where(e => e.RoleKey == "mentor"),
            "reviewer" => entries.Where(e => e.RoleKey == "reviewer"),
            _ => entries
        };
    }

    private static IEnumerable<LecturerScheduleEntryDto> ApplyRangeFilter(
        IEnumerable<LecturerScheduleEntryDto> entries,
        string rangeFilter,
        DateTime now)
    {
        var today = now.Date;
        var weekStart = GetStartOfWeek(today, DayOfWeek.Monday);
        var weekEnd = weekStart.AddDays(6);

        return rangeFilter switch
        {
            "today" => entries.Where(e => e.ScheduledAt.Date == today),
            "upcoming" => entries.Where(e => e.ScheduledAt >= now),
            "past" => entries.Where(e => e.ScheduledAt < now),
            "all" => entries,
            _ => entries.Where(e => e.ScheduledAt.Date >= weekStart && e.ScheduledAt.Date <= weekEnd)
        };
    }

    private static List<LecturerScheduleDayGroupDto> BuildDayGroups(
        IEnumerable<LecturerScheduleEntryDto> entries,
        DateTime now)
    {
        return entries
            .GroupBy(e => e.ScheduledAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new LecturerScheduleDayGroupDto
            {
                Date = g.Key,
                Label = BuildDayLabel(g.Key, now.Date),
                Entries = g.OrderByDescending(e => e.NeedsAttention).ThenBy(e => e.ScheduledAt).ToList()
            })
            .ToList();
    }

    private static List<LecturerScheduleWeekDayDto> BuildWeekDays(
        IEnumerable<LecturerScheduleEntryDto> entries,
        DateTime weekStart,
        DateTime now)
    {
        var lookup = entries
            .GroupBy(e => e.ScheduledAt.Date)
            .ToDictionary(g => g.Key, g => g.OrderBy(e => e.ScheduledAt).ToList());

        return Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = weekStart.AddDays(offset);
                return new LecturerScheduleWeekDayDto
                {
                    Date = date,
                    DayLabel = $"{date:ddd} {date:dd}",
                    IsToday = date.Date == now.Date,
                    Entries = lookup.TryGetValue(date.Date, out var dayEntries)
                        ? dayEntries
                        : new List<LecturerScheduleEntryDto>()
                };
            })
            .ToList();
    }

    private static LecturerScheduleFocusDto? BuildFocusCard(
        IReadOnlyCollection<LecturerScheduleEntryDto> entries,
        IReadOnlyCollection<LecturerDeadlineDto> deadlines,
        DateTime now)
    {
        var priorityEntry = entries
            .OrderByDescending(e => e.NeedsAttention)
            .ThenBy(e => e.ScheduledAt)
            .FirstOrDefault(e => e.NeedsAttention)
            ?? entries.FirstOrDefault(e => e.StatusKey == "live")
            ?? entries.FirstOrDefault(e => e.ScheduledAt >= now);

        if (priorityEntry != null)
        {
            return new LecturerScheduleFocusDto
            {
                Eyebrow = priorityEntry.NeedsAttention ? "Needs attention" : priorityEntry.StatusLabel,
                Title = $"{priorityEntry.GroupName} - Round {priorityEntry.RoundNumber}",
                Description = $"{priorityEntry.RoleLabel} session for {priorityEntry.ProjectName}. {priorityEntry.Guidance}",
                ActionText = priorityEntry.PrimaryActionText,
                ActionUrl = priorityEntry.PrimaryActionUrl,
                SecondaryActionText = priorityEntry.SecondaryActionText,
                SecondaryActionUrl = priorityEntry.SecondaryActionUrl
            };
        }

        var nextDeadline = deadlines.OrderBy(d => d.DueAt).FirstOrDefault();
        if (nextDeadline != null)
        {
            return new LecturerScheduleFocusDto
            {
                Eyebrow = "Upcoming deadline",
                Title = nextDeadline.Title,
                Description = nextDeadline.Description,
                ActionText = nextDeadline.ActionText,
                ActionUrl = nextDeadline.ActionUrl
            };
        }

        return null;
    }

    private static DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
    {
        var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }

    private static bool IsLiveSession(DateTime scheduledAt, DateTime now)
    {
        return scheduledAt <= now && scheduledAt.AddMinutes(60) >= now;
    }

    private static string BuildDayLabel(DateTime date, DateTime today)
    {
        if (date == today)
        {
            return "Today";
        }

        if (date == today.AddDays(1))
        {
            return "Tomorrow";
        }

        if (date == today.AddDays(-1))
        {
            return "Yesterday";
        }

        return date.ToString("dddd, dd MMM");
    }

    private static string BuildTimeHint(DateTime scheduledAt, DateTime now)
    {
        var diff = scheduledAt - now;

        if (IsLiveSession(scheduledAt, now))
        {
            return "In progress";
        }

        if (scheduledAt.Date == now.Date && diff.TotalMinutes > 0)
        {
            return diff.TotalHours < 1
                ? $"Starts in {Math.Max(1, (int)Math.Ceiling(diff.TotalMinutes))} min"
                : $"Starts in {(int)Math.Ceiling(diff.TotalHours)}h";
        }

        if (scheduledAt.Date == now.Date && diff.TotalMinutes < 0)
        {
            return "Earlier today";
        }

        if (scheduledAt.Date == now.Date.AddDays(1))
        {
            return "Tomorrow";
        }

        if (diff.TotalDays > 1)
        {
            return $"In {(int)Math.Ceiling(diff.TotalDays)} days";
        }

        if (diff.TotalDays < -1)
        {
            return $"Occurred on {scheduledAt:dd MMM}";
        }

        return scheduledAt.ToString("ddd, HH:mm");
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

    private static void ApplyMentorChecklistComments(Evaluation evaluation, IEnumerable<MentorChecklistCommentDto> itemComments)
    {
        foreach (var itemComment in itemComments)
        {
            var detail = evaluation.EvaluationDetails.FirstOrDefault(d => d.ItemID == itemComment.ItemId);
            if (detail != null)
            {
                detail.MentorComment = itemComment.MentorComment?.Trim();
            }
        }
    }



    private static string? NormalizeAssessmentValue(ChecklistItem item, string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return string.Empty;
        }

        var normalized = rawValue.Trim();

        if (string.Equals(item.ItemType, "NumericScore", StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        var allowedValues = IsGradeChecklistItem(item)
            ? new[] { "Excellent", "Good", "Acceptable", "Fail", "NA" }
            : new[] { "Y", "N", "NA" };

        return allowedValues.FirstOrDefault(value => value.Equals(normalized, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }

    private static string? ResolveGradeDescription(ChecklistItem item, string? assessmentValue)
    {
        return item.RubricDescriptions.FirstOrDefault(r => r.GradeLevel == assessmentValue)?.Description;
    }

    private static bool IsGradeChecklistItem(ChecklistItem item)
    {
        return string.Equals(item.ItemType, "Grade", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(item.ItemType, "GradeLevel", StringComparison.OrdinalIgnoreCase) ||
               item.RubricDescriptions.Any();
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
    public async Task<(byte[] content, string fileName, string contentType)?> GetSubmissionFileAsync(int submissionId)
    {
        var submission = await _submissionRepo.GetByIdAsync(submissionId);
        if (submission == null || string.IsNullOrEmpty(submission.FileUrl))
            return null;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(submission.FileUrl);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            
            return (content, submission.FileName, contentType);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> CanUserAccessSubmissionAsync(string userId, int submissionId, string role)
    {
        var submission = await _submissionRepo.GetByIdAsync(submissionId);
        if (submission == null) return false;

        if (role == "HeadOfDept") return true;

        if (role == "Student")
        {
            // Verify student is in the group associated with the submission
            return submission.Group.GroupMembers.Any(m => m.UserID == userId);
        }

        if (role == "Lecturer")
        {
            // Supervisor check
            if (submission.Group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == userId)) return true;

            // Reviewer check
            var assignments = await _assignmentRepo.GetByReviewerAsync(userId);
            return assignments.Any(a => a.GroupID == submission.GroupID && a.ReviewRoundID == submission.Requirement.ReviewRoundID);
        }

        return false;
    }
}
