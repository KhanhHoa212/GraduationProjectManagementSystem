using GPMS.Application.DTOs.Lecturer;
using GPMS.Domain.Enums;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface ILecturerService
{
    Task<LecturerDashboardDto> GetDashboardDataAsync(string lecturerId);
    Task<LecturerProjectsDto> GetMentoredProjectsAsync(string lecturerId);
    Task<LecturerProjectGroupDetailDto> GetProjectGroupDetailAsync(string lecturerId, int groupId);
    Task<LecturerFeedbackApprovalsDto> GetPendingApprovalsAsync(string lecturerId);
    Task<LecturerFeedbackApprovalDetailDto> GetFeedbackApprovalDetailAsync(string lecturerId, int feedbackId);
    Task<LecturerReviewAssignmentsDto> GetReviewAssignmentsAsync(string reviewerId);
    Task<LecturerScheduleDto> GetScheduleAsync(string lecturerId, string? roleFilter = null, string? rangeFilter = null, int weekOffset = 0);
    Task<LecturerHistoryDto> GetHistoryAsync(string lecturerId);
    Task<LecturerEvaluationFormDto?> GetEvaluationFormAsync(string reviewerId, int assignmentId);
    Task<(bool Success, string ErrorMessage)> SubmitEvaluationAsync(string reviewerId, EvaluationSubmitDto model);
    Task<(bool Success, string ErrorMessage)> ReviewRoundGateAsync(string supervisorId, int groupId, int roundId, MentorGateStatus decision, string? progressComment);
    Task<bool> ApproveFeedbackAsync(string supervisorId, FeedbackApprovalDecisionDto model);
    Task<(byte[] content, string fileName, string contentType)?> GetSubmissionFileAsync(int submissionId);
    Task<bool> CanUserAccessSubmissionAsync(string userId, int submissionId, string role);
}
