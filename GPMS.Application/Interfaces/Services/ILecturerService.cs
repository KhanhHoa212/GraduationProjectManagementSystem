using GPMS.Application.DTOs.Lecturer;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface ILecturerService
{
    Task<LecturerDashboardViewModel> GetDashboardDataAsync(string lecturerId);
    Task<LecturerProjectsViewModel> GetMentoredProjectsAsync(string lecturerId);
    Task<LecturerProjectGroupDetailViewModel> GetProjectGroupDetailAsync(int groupId);
    Task<LecturerFeedbackApprovalsViewModel> GetPendingApprovalsAsync(string lecturerId);
    Task<LecturerFeedbackApprovalDetailViewModel> GetFeedbackApprovalDetailAsync(int feedbackId);
    Task<LecturerReviewAssignmentsViewModel> GetReviewAssignmentsAsync(string reviewerId);
    Task<LecturerEvaluationFormViewModel> GetEvaluationFormAsync(int assignmentId);
    Task<bool> SubmitEvaluationAsync(LecturerEvaluationFormViewModel model);
    Task<bool> ApproveFeedbackAsync(int feedbackId, string decision, string comments);
}
