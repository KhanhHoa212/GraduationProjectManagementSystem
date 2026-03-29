using GPMS.Application.DTOs.Lecturer;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;

namespace GPMS.Application.Services;

public class LecturerWorkflowService : ILecturerWorkflowService
{
    private readonly IProjectGroupRepository _groupRepo;
    private readonly IReviewerAssignmentRepository _assignmentRepo;
    private readonly IFeedbackRepository _feedbackRepo;
    private readonly IEvaluationRepository _evaluationRepo;
    private readonly IMentorRoundReviewRepository _mentorRoundReviewRepo;
    private readonly INotificationRepository _notificationRepo;
    private readonly IEmailService _emailService;

    public LecturerWorkflowService(
        IProjectGroupRepository groupRepo,
        IReviewerAssignmentRepository assignmentRepo,
        IFeedbackRepository feedbackRepo,
        IEvaluationRepository evaluationRepo,
        IMentorRoundReviewRepository mentorRoundReviewRepo,
        INotificationRepository notificationRepo,
        IEmailService emailService)
    {
        _groupRepo = groupRepo;
        _assignmentRepo = assignmentRepo;
        _feedbackRepo = feedbackRepo;
        _evaluationRepo = evaluationRepo;
        _mentorRoundReviewRepo = mentorRoundReviewRepo;
        _notificationRepo = notificationRepo;
        _emailService = emailService;
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
            MeetLink = session?.MeetLink,
            SubmissionFileName = submission?.FileName,
            SubmissionUrl = submission?.FileUrl,
            Members = assignment.Group?.GroupMembers.Select(LecturerPresentationHelper.MapToStudentMemberDto).ToList() ?? new List<StudentMemberDto>(),
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

}
