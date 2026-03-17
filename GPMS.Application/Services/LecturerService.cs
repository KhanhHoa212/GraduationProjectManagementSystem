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

    public LecturerService(
        IProjectGroupRepository groupRepo,
        IReviewerAssignmentRepository assignmentRepo,
        IFeedbackRepository feedbackRepo,
        IReviewRoundRepository roundRepo,
        IEvaluationRepository evaluationRepo)
    {
        _groupRepo = groupRepo;
        _assignmentRepo = assignmentRepo;
        _feedbackRepo = feedbackRepo;
        _roundRepo = roundRepo;
        _evaluationRepo = evaluationRepo;
    }

    public async Task<LecturerDashboardDto> GetDashboardDataAsync(string lecturerId)
    {
        var groups = (await _groupRepo.GetBySupervisorAsync(lecturerId)).ToList();
        var pendingFeedbacks = (await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId)).ToList();
        var assignments = (await _assignmentRepo.GetByReviewerAsync(lecturerId)).ToList();
        var today = DateTime.Today;

        var assignmentItems = assignments.Select(a =>
        {
            var session = a.Group?.ReviewSessions?.FirstOrDefault(rs => rs.ReviewRoundID == a.ReviewRoundID);
            var evaluation = a.Group?.Evaluations?.FirstOrDefault(e =>
                e.ReviewRoundID == a.ReviewRoundID &&
                e.ReviewerID == lecturerId);

            return new
            {
                Assignment = a,
                Session = session,
                Evaluation = evaluation
            };
        }).ToList();

        var recentActivities = pendingFeedbacks
            .Select(f => new DashboardActivityItemDto
            {
                Title = $"{f.Evaluation?.Group?.GroupName ?? "Group"} awaiting approval",
                Description = $"Review feedback for {f.Evaluation?.Group?.Project?.ProjectName ?? "project"}.",
                Timestamp = f.CreatedAt,
                Icon = "fact_check",
                IconBgColor = "var(--fpt-orange)",
                ActionUrl = $"/Lecturer/FeedbackApprovalDetail/{f.FeedbackID}",
                ActionText = "Review Now"
            })
            .Concat(assignmentItems.Select(x => new DashboardActivityItemDto
            {
                Title = $"Review assignment: {x.Assignment.Group?.GroupName ?? "Group"}",
                Description = $"Round {x.Assignment.ReviewRound?.RoundNumber ?? 0} for {x.Assignment.Group?.Project?.ProjectName ?? "project"}.",
                Timestamp = x.Assignment.AssignedAt,
                Icon = x.Evaluation == null ? "assignment" : "task_alt",
                IconBgColor = x.Evaluation == null ? "#6C757D" : "#16A34A",
                ActionUrl = $"/Lecturer/EvaluationForm/{x.Assignment.AssignmentID}",
                ActionText = x.Evaluation == null ? "Start Review" : "View Review"
            }))
            .OrderByDescending(a => a.Timestamp)
            .Take(5)
            .ToList();

        var todaysSchedule = assignmentItems
            .Where(x => x.Session?.ScheduledAt.Date == today)
            .OrderBy(x => x.Session!.ScheduledAt)
            .Select(x => new DashboardScheduleItemDto
            {
                Title = $"Review {x.Assignment.Group?.GroupName ?? "Group"}",
                Location = !string.IsNullOrWhiteSpace(x.Session?.MeetLink)
                    ? "Online meeting"
                    : x.Session?.Room?.RoomCode ?? "TBD",
                StartTime = x.Session!.ScheduledAt,
                DurationMinutes = 60,
                IsHighlight = x.Evaluation == null
            })
            .ToList();

        return new LecturerDashboardDto
        {
            MentoringGroupsCount = groups.Count,
            PendingApprovalsCount = pendingFeedbacks.Count,
            AssignedReviewsCount = assignments.Count,
            UpcomingDeadlinesCount = assignmentItems.Count(x =>
                x.Evaluation == null &&
                x.Session?.ScheduledAt >= today &&
                x.Session.ScheduledAt < today.AddDays(7)),
            RecentActivities = recentActivities,
            TodaysSchedule = todaysSchedule
        };
    }

    public async Task<LecturerProjectsDto> GetMentoredProjectsAsync(string lecturerId)
    {
        var groups = (await _groupRepo.GetBySupervisorAsync(lecturerId)).ToList();
        var dto = new LecturerProjectsDto();

        foreach (var group in groups)
        {
            var submittedEvaluations = group.Evaluations.Count(e => e.Status == EvaluationStatus.Submitted);
            var totalRounds = Math.Max(1, submittedEvaluations + 1);
            var progressPercent = Math.Min(100, submittedEvaluations * 100 / totalRounds);
            var pendingApproval = group.Evaluations.Any(e =>
                e.Feedback?.FeedbackApproval?.ApprovalStatus == ApprovalStatus.Pending);

            dto.Projects.Add(new LecturerProjectItemDto
            {
                GroupId = group.GroupID,
                ProjectName = group.Project?.ProjectName ?? "N/A",
                GroupName = group.GroupName,
                Semester = group.Project?.Semester?.SemesterCode ?? string.Empty,
                SupervisorRole = group.Project?.ProjectSupervisors
                    .FirstOrDefault(ps => ps.LecturerID == lecturerId)?.Role.ToString() ?? ProjectRole.Main.ToString(),
                MemberNames = group.GroupMembers.Select(m => m.User?.FullName ?? "Unknown").ToList(),
                CurrentRound = submittedEvaluations > 0 ? $"Round {submittedEvaluations}" : "Not reviewed yet",
                Status = pendingApproval ? "Awaiting approval" : "Active",
                ProgressPercent = progressPercent
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

        var pendingFeedbacks = await _feedbackRepo.GetPendingApprovalsBySupervisorAsync(lecturerId);
        var groupFeedback = pendingFeedbacks
            .Where(f => f.Evaluation?.GroupID == groupId)
            .OrderByDescending(f => f.CreatedAt)
            .FirstOrDefault();

        return new LecturerProjectGroupDetailDto
        {
            GroupId = group.GroupID,
            GroupName = group.GroupName,
            ProjectName = group.Project?.ProjectName ?? "N/A",
            Semester = group.Project?.Semester?.SemesterCode ?? string.Empty,
            PendingFeedbackId = groupFeedback?.FeedbackID,
            Members = group.GroupMembers.Select(MapStudentMember).ToList()
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
                AutoReleaseAt = feedback.FeedbackApproval?.AutoReleasedAt,
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
        var totalRounds = await GetSemesterRoundCountAsync(group.Project.SemesterID);

        var scores = evaluation.EvaluationDetails
            .OrderBy(s => s.Item.OrderIndex)
            .Select(s => new EvaluationScoreItemDto
            {
                ItemCode = s.Item.ItemCode,
                CriteriaName = s.Item.ItemContent,
                Score = s.Score,
                MaxScore = s.Item.MaxScore,
                WeightPercentage = s.Item.Weight
            })
            .ToList();

        return new LecturerFeedbackApprovalDetailDto
        {
            FeedbackId = feedback.FeedbackID,
            EvaluationId = feedback.EvaluationID,
            GroupName = group.GroupName,
            GroupId = group.GroupID,
            ReviewRoundName = evaluation.ReviewRound.RoundNumber.ToString(),
            CurrentRoundIndex = evaluation.ReviewRound.RoundNumber,
            TotalRounds = totalRounds,
            SubmittedAt = evaluation.SubmittedAt ?? feedback.CreatedAt,
            ApprovalStatus = feedback.FeedbackApproval?.ApprovalStatus ?? ApprovalStatus.Pending,
            SupervisorComment = feedback.FeedbackApproval?.SupervisorComment,
            ReviewerName = evaluation.Reviewer.FullName,
            FeedbackContent = feedback.Content,
            TotalScore = evaluation.TotalScore ?? 0m,
            MaxTotalScore = scores.Any() ? scores.Sum(s => s.MaxScore) : 0m,
            Scores = scores,
            Members = group.GroupMembers.Select(MapStudentMember).ToList()
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
                Location = !string.IsNullOrWhiteSpace(session?.MeetLink)
                    ? "Online meeting"
                    : session?.Room?.RoomCode,
                IsOnline = !string.IsNullOrWhiteSpace(session?.MeetLink),
                HasEvaluation = evaluation?.Status == EvaluationStatus.Submitted,
                EvaluationId = evaluation?.EvaluationID
            });
        }

        dto.PendingEvaluationsCount = dto.Assignments.Count(a => !a.HasEvaluation);
        dto.ScheduledTodayCount = dto.Assignments.Count(a => a.ScheduledAt?.Date == DateTime.Today);
        dto.CompletedReviewsCount = dto.Assignments.Count(a => a.HasEvaluation);

        return dto;
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
            Members = assignment.Group?.GroupMembers.Select(MapStudentMember).ToList() ?? new List<StudentMemberDto>(),
            ChecklistItems = assignment.ReviewRound?.ReviewChecklist?.ChecklistItems
                .OrderBy(ci => ci.OrderIndex)
                .Select(ci => new ChecklistItemDto
                {
                    ItemId = ci.ItemID,
                    ItemCode = ci.ItemCode,
                    ItemContent = ci.ItemContent,
                    MaxScore = ci.MaxScore,
                    Weight = ci.Weight
                })
                .ToList() ?? new List<ChecklistItemDto>(),
            ExistingEvaluationId = existingEvaluation?.EvaluationID,
            ExistingFeedbackContent = existingEvaluation?.Feedback?.Content,
            ExistingScores = existingEvaluation?.EvaluationDetails
                .Select(d => new ExistingScoreDto
                {
                    ItemId = d.ItemID,
                    Score = d.Score,
                    Comment = d.Comment
                })
                .ToList() ?? new List<ExistingScoreDto>()
        };
    }

    public async Task<bool> SubmitEvaluationAsync(string reviewerId, EvaluationSubmitDto model)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(model.AssignmentId);
        if (assignment == null || assignment.ReviewerID != reviewerId)
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

            var normalizedScore = Math.Min(item.MaxScore, Math.Max(0m, input.Score));
            normalizedScores.Add(new ScoreInputDto
            {
                CriteriaId = item.ItemID,
                Score = normalizedScore,
                Comment = input.Comment?.Trim()
            });
        }

        var totalScore = normalizedScores.Sum(s => s.Score);
        var supervisorId = assignment.Group?.Project?.ProjectSupervisors
            .OrderBy(ps => ps.Role == ProjectRole.Main ? 0 : 1)
            .Select(ps => ps.LecturerID)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(supervisorId))
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
                TotalScore = totalScore,
                Status = EvaluationStatus.Submitted,
                SubmittedAt = now,
                EvaluationDetails = normalizedScores.Select(score => new EvaluationDetail
                {
                    ItemID = score.CriteriaId,
                    Score = score.Score,
                    Comment = score.Comment
                }).ToList(),
                Feedback = new Feedback
                {
                    Content = model.OverallFeedback.Trim(),
                    CreatedAt = now,
                    FeedbackApproval = new FeedbackApproval
                    {
                        SupervisorID = supervisorId,
                        ApprovalStatus = ApprovalStatus.Pending,
                        SupervisorComment = null,
                        ApprovedAt = null,
                        AutoReleasedAt = null,
                        IsVisibleToStudent = false
                    }
                }
            };

            await _evaluationRepo.AddAsync(evaluation);
            await _evaluationRepo.SaveChangesAsync();
            return true;
        }

        if (existingEvaluation.Status == EvaluationStatus.Submitted)
        {
            return false;
        }

        existingEvaluation.TotalScore = totalScore;
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
                    Score = score.Score,
                    Comment = score.Comment
                });
                continue;
            }

            detail.Score = score.Score;
            detail.Comment = score.Comment;
        }

        if (existingEvaluation.Feedback == null)
        {
            existingEvaluation.Feedback = new Feedback
            {
                Content = model.OverallFeedback.Trim(),
                CreatedAt = now,
                FeedbackApproval = new FeedbackApproval
                {
                    SupervisorID = supervisorId,
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
                    SupervisorID = supervisorId,
                    ApprovalStatus = ApprovalStatus.Pending,
                    IsVisibleToStudent = false
                };
            }
            else
            {
                existingEvaluation.Feedback.FeedbackApproval.SupervisorID = supervisorId;
                existingEvaluation.Feedback.FeedbackApproval.ApprovalStatus = ApprovalStatus.Pending;
                existingEvaluation.Feedback.FeedbackApproval.SupervisorComment = null;
                existingEvaluation.Feedback.FeedbackApproval.ApprovedAt = null;
                existingEvaluation.Feedback.FeedbackApproval.AutoReleasedAt = null;
                existingEvaluation.Feedback.FeedbackApproval.IsVisibleToStudent = false;
            }
        }

        _evaluationRepo.Update(existingEvaluation);
        await _evaluationRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveFeedbackAsync(string supervisorId, int feedbackId, string decision, string comments)
    {
        var feedback = await _feedbackRepo.GetByIdWithDetailsAsync(feedbackId);
        if (feedback == null || feedback.FeedbackApproval == null)
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
        var now = DateTime.UtcNow;

        switch (decision)
        {
            case "Approve":
                approval.ApprovalStatus = ApprovalStatus.Approved;
                approval.SupervisorComment = null;
                approval.ApprovedAt = now;
                approval.AutoReleasedAt = now;
                approval.IsVisibleToStudent = true;
                break;

            case "ApproveWithEdits":
                if (!string.IsNullOrWhiteSpace(comments))
                {
                    feedback.Content = comments.Trim();
                }

                approval.ApprovalStatus = ApprovalStatus.Approved;
                approval.SupervisorComment = "Approved with edits.";
                approval.ApprovedAt = now;
                approval.AutoReleasedAt = now;
                approval.IsVisibleToStudent = true;
                break;

            case "Reject":
                if (string.IsNullOrWhiteSpace(comments))
                {
                    return false;
                }

                approval.ApprovalStatus = ApprovalStatus.Rejected;
                approval.SupervisorComment = comments.Trim();
                approval.ApprovedAt = null;
                approval.AutoReleasedAt = null;
                approval.IsVisibleToStudent = false;
                break;

            default:
                return false;
        }

        await _feedbackRepo.UpdateApprovalAsync(approval);
        await _feedbackRepo.SaveChangesAsync();
        return true;
    }

    private async Task<int> GetSemesterRoundCountAsync(int semesterId)
    {
        var rounds = await _roundRepo.GetBySemesterAsync(semesterId);
        return Math.Max(1, rounds.Count());
    }

    private static StudentMemberDto MapStudentMember(GroupMember member)
    {
        var fullName = member.User?.FullName ?? "Unknown";
        return new StudentMemberDto
        {
            UserId = member.UserID,
            FullName = fullName,
            RoleInGroup = member.RoleInGroup.ToString(),
            AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(fullName)}&background=E5E7EB&color=374151"
        };
    }
}
