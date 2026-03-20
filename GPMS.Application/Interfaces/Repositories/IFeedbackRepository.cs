using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace GPMS.Application.Interfaces.Repositories;

public interface IFeedbackRepository
{
    Task<Feedback?> GetByIdAsync(int feedbackId);
    Task<Feedback?> GetByEvaluationIdAsync(int evaluationId);
    Task<IEnumerable<Feedback>> GetBySupervisorAsync(string supervisorId);
    Task<IEnumerable<Feedback>> GetPendingApprovalsBySupervisorAsync(string supervisorId);
    Task<IEnumerable<Feedback>> GetVisibleFeedbacksForStudentAsync(string studentId);
    Task<Feedback?> GetByIdWithDetailsAsync(int feedbackId);
    Task<IEnumerable<Feedback>> GetRecentFeedbacksByStudentAsync(string studentId, int count);
    Task<IReadOnlyList<FeedbackApproval>> GetApprovalsPendingAutoReleaseAsync(DateTime approvedBeforeUtc, CancellationToken cancellationToken = default);
    Task AddAsync(Feedback feedback);
    Task UpdateApprovalAsync(FeedbackApproval approval);
    Task SaveChangesAsync();
}
