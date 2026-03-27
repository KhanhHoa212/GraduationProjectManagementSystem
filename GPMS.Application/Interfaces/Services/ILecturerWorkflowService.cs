using GPMS.Application.DTOs.Lecturer;
using GPMS.Domain.Enums;

namespace GPMS.Application.Interfaces.Services;

public interface ILecturerWorkflowService
{
    Task<LecturerEvaluationFormDto?> GetEvaluationFormAsync(string reviewerId, int assignmentId);
    Task<(bool Success, string ErrorMessage)> SubmitEvaluationAsync(string reviewerId, EvaluationSubmitDto model);
    Task<(bool Success, string ErrorMessage)> ReviewRoundGateAsync(string supervisorId, int groupId, int roundId, MentorGateStatus decision, string? progressComment);
    Task<bool> ApproveFeedbackAsync(string supervisorId, FeedbackApprovalDecisionDto model);
}
