using GPMS.Application.DTOs.Lecturer;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IFeedbackRepository
{
    // Entity methods (write-path — needed for update/approval operations)
    Task<Feedback?> GetByIdAsync(int feedbackId);
    Task<Feedback?> GetByEvaluationIdAsync(int evaluationId);
    Task<IEnumerable<Feedback>> GetBySupervisorAsync(string supervisorId);
    Task<IEnumerable<Feedback>> GetPendingApprovalsBySupervisorAsync(string supervisorId);
    Task<Feedback?> GetByIdWithDetailsAsync(int feedbackId);
    Task<IEnumerable<Feedback>> GetRecentFeedbacksByStudentAsync(string studentId, int count);
    Task<IReadOnlyList<FeedbackApproval>> GetApprovalsPendingAutoReleaseAsync(DateTime approvedBeforeUtc, CancellationToken cancellationToken = default);
    Task<IEnumerable<Feedback>> GetVisibleFeedbacksForStudentAsync(string studentId);
    Task AddAsync(Feedback feedback);
    Task UpdateApprovalAsync(FeedbackApproval approval);
    Task SaveChangesAsync();

    // DTO Projection methods (read-only — no entity tracking)
    Task<IEnumerable<PendingFeedbackItemDto>> GetPendingApprovalDtosBySupervisorAsync(string supervisorId);
    Task<LecturerFeedbackApprovalDetailDto?> GetApprovalDetailDtoAsync(int feedbackId);
    Task<IEnumerable<LecturerFeedbackHistoryItemDto>> GetHistoryDtosBySupervisorAsync(string supervisorId);
}
