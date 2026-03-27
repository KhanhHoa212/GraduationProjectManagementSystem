using GPMS.Application.DTOs.Lecturer;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;

namespace GPMS.Application.Services;

public class LecturerScheduleService : ILecturerScheduleService
{
    private readonly IReviewRoundRepository _roundRepo;

    public LecturerScheduleService(IReviewRoundRepository roundRepo)
    {
        _roundRepo = roundRepo;
    }

    public IReadOnlyList<LecturerScheduleEntryDto> BuildEntries(
        IEnumerable<ProjectGroupSummaryDto> groups,
        IEnumerable<ReviewAssignmentItemDto> assignments,
        DateTime now)
    {
        var mentorEntries = groups.SelectMany(group => group.Sessions.Select(session =>
        {
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
                IsOnline = session.IsOnline,
                Location = ResolveLocation(session),
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

                var hasSubmitted = assignment.HasEvaluation;
                var isBlockedByMentor = assignment.StatusNote == "Blocked by Mentor";
                var needsRevision = assignment.StatusNote == "Needs Revision";
                var isApproved = assignment.ApprovalStatus == ApprovalStatus.Approved ||
                                 assignment.ApprovalStatus == ApprovalStatus.AutoReleased;
                var waitingMentor = assignment.StatusNote == "Waiting for Mentor Approval" ||
                                    (hasSubmitted && !needsRevision && !isBlockedByMentor && !isApproved &&
                                     (assignment.ApprovalStatus == ApprovalStatus.Pending || assignment.StatusNote == null));
                var needsEvaluation = !isBlockedByMentor && assignment.ScheduledAt.Value < now && (!hasSubmitted || needsRevision);
                var isLive = IsLiveSession(assignment.ScheduledAt.Value, now);

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
                    Location = assignment.Location ?? (assignment.IsOnline ? "Online session" : "Location pending"),
                    Guidance = isBlockedByMentor
                        ? "Mentor has blocked this round. Review the mentor note before editing anything."
                        : needsRevision
                            ? "Update the evaluation with the mentor's requested changes."
                            : needsEvaluation
                                ? "The session has passed. Please complete and submit the evaluation."
                                : waitingMentor
                                    ? "Your evaluation is submitted and waiting for mentor approval."
                                    : "Prepare rubric notes before the session and submit the evaluation afterward.",
                    StatusKey = isBlockedByMentor
                        ? "blocked-mentor"
                        : needsRevision
                            ? "needs-revision"
                            : needsEvaluation
                                ? "needs-evaluation"
                                : waitingMentor
                                    ? "waiting-mentor"
                                    : hasSubmitted && isApproved
                                        ? "completed"
                                        : hasSubmitted
                                            ? "waiting-mentor"
                                            : isLive
                                                ? "live"
                                                : assignment.ScheduledAt.Value.Date == now.Date
                                                    ? "today"
                                                    : "upcoming",
                    StatusLabel = isBlockedByMentor
                        ? "Blocked by mentor"
                        : needsRevision
                            ? "Needs revision"
                            : needsEvaluation
                                ? "Need evaluation"
                                : waitingMentor
                                    ? "Waiting mentor approval"
                                    : hasSubmitted && isApproved
                                        ? "Completed"
                                        : hasSubmitted
                                            ? "Waiting mentor approval"
                                            : isLive
                                                ? "Live now"
                                                : assignment.ScheduledAt.Value.Date == now.Date
                                                    ? "Today"
                                                    : "Upcoming",
                    StatusTone = isBlockedByMentor || needsRevision || needsEvaluation
                        ? "danger"
                        : waitingMentor || isLive
                            ? "warning"
                            : hasSubmitted && isApproved
                                ? "success"
                                : "info",
                    NeedsAttention = isBlockedByMentor || needsRevision || needsEvaluation,
                    IsToday = assignment.ScheduledAt.Value.Date == now.Date,
                    IsPast = assignment.ScheduledAt.Value < now,
                    TimeHint = BuildTimeHint(assignment.ScheduledAt.Value, now),
                    PrimaryActionText = isBlockedByMentor
                        ? "View evaluation"
                        : needsRevision
                            ? "Revise evaluation"
                            : "Open evaluation",
                    PrimaryActionUrl = $"/Lecturer/EvaluationForm/{assignment.AssignmentId}"
                };
            })
            .Where(entry => entry != null)!
            .Select(entry => entry!)
            .ToList();

        return mentorEntries.Concat(reviewerEntries).OrderBy(e => e.ScheduledAt).ToList();
    }

    public async Task<IReadOnlyList<LecturerDeadlineDto>> BuildDeadlinesAsync(
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
            if (!roundsBySemester.TryGetValue(group.SemesterId, out var rounds))
            {
                rounds = (await _roundRepo.GetBySemesterAsync(group.SemesterId)).ToList();
                roundsBySemester[group.SemesterId] = rounds;
            }

            foreach (var round in rounds)
            {
                foreach (var requirement in round.SubmissionRequirements.Where(r => r.Deadline.Date >= today))
                {
                    var hasSubmission = group.Submissions.Any(s => s.RequirementId == requirement.RequirementID);
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

    public async Task<LecturerScheduleDto> BuildScheduleAsync(
        IEnumerable<ProjectGroupSummaryDto> groups,
        IEnumerable<ReviewAssignmentItemDto> assignments,
        IEnumerable<PendingFeedbackItemDto> pendingFeedbacks,
        string? roleFilter,
        string? rangeFilter,
        int weekOffset,
        DateTime now)
    {
        var normalizedRoleFilter = NormalizeRoleFilter(roleFilter);
        var normalizedRangeFilter = NormalizeRangeFilter(rangeFilter);

        var allEntries = BuildEntries(groups, assignments, now)
            .OrderByDescending(s => s.NeedsAttention)
            .ThenBy(s => s.ScheduledAt)
            .ToList();
        var roleScopedEntries = ApplyRoleFilter(allEntries, normalizedRoleFilter).ToList();
        var filteredEntries = ApplyRangeFilter(roleScopedEntries, normalizedRangeFilter, now)
            .OrderByDescending(s => s.NeedsAttention)
            .ThenBy(s => s.ScheduledAt)
            .ToList();
        var deadlines = normalizedRoleFilter == "reviewer"
            ? new List<LecturerDeadlineDto>()
            : (await BuildDeadlinesAsync(groups, pendingFeedbacks, now)).ToList();
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
            ActiveRoleFilter = normalizedRoleFilter,
            ActiveRangeFilter = normalizedRangeFilter,
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

    private static string ResolveLocation(GroupSessionSummaryDto session)
    {
        if (session.IsOnline)
        {
            return "Online session";
        }

        if (string.IsNullOrWhiteSpace(session.RoomCode))
        {
            return "Location pending";
        }

        return string.IsNullOrWhiteSpace(session.Building)
            ? session.RoomCode
            : $"{session.RoomCode} - {session.Building}";
    }
}
