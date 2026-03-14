using GPMS.Application.DTOs.Lecturer;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface ILecturerService
{
    Task<LecturerDashboardDto> GetDashboardDataAsync(string lecturerId);
    Task<LecturerProjectsDto> GetMentoredProjectsAsync(string lecturerId);
    Task<LecturerProjectGroupDetailDto> GetProjectGroupDetailAsync(int groupId);
    Task<LecturerFeedbackApprovalsDto> GetPendingApprovalsAsync(string lecturerId);
    Task<LecturerFeedbackApprovalDetailDto> GetFeedbackApprovalDetailAsync(int feedbackId);
    Task<LecturerReviewAssignmentsDto> GetReviewAssignmentsAsync(string reviewerId);
    Task<LecturerEvaluationFormDto> GetEvaluationFormAsync(int assignmentId);
    Task<bool> SubmitEvaluationAsync(EvaluationSubmitDto model);
    Task<bool> ApproveFeedbackAsync(int feedbackId, string decision, string comments);
}
