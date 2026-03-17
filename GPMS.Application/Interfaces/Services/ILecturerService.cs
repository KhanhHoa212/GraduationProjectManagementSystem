using GPMS.Application.DTOs.Lecturer;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface ILecturerService
{
    Task<LecturerDashboardDto> GetDashboardDataAsync(string lecturerId);
    Task<LecturerProjectsDto> GetMentoredProjectsAsync(string lecturerId);
    Task<LecturerProjectGroupDetailDto> GetProjectGroupDetailAsync(string lecturerId, int groupId);
    Task<LecturerFeedbackApprovalsDto> GetPendingApprovalsAsync(string lecturerId);
    Task<LecturerFeedbackApprovalDetailDto> GetFeedbackApprovalDetailAsync(int feedbackId);
    Task<LecturerReviewAssignmentsDto> GetReviewAssignmentsAsync(string reviewerId);
    Task<LecturerEvaluationFormDto?> GetEvaluationFormAsync(string reviewerId, int assignmentId);
    Task<bool> SubmitEvaluationAsync(string reviewerId, EvaluationSubmitDto model);
    Task<bool> ApproveFeedbackAsync(string supervisorId, int feedbackId, string decision, string comments);
}
