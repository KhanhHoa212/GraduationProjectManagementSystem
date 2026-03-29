using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IReviewSessionRepository
{
    Task<IEnumerable<ReviewSessionInfo>> GetByRoundAndDateAsync(int roundId, DateTime date);
    Task<IEnumerable<ReviewSessionInfo>> GetByDateAsync(DateTime date);
    Task<IEnumerable<ReviewSessionInfo>> GetByRoundAsync(int roundId);
    Task<ReviewSessionInfo?> GetByRoundAndGroupAsync(int roundId, int groupId);
    Task<ReviewSessionInfo?> GetByIdAsync(int sessionId);
    Task<List<ReviewSessionInfo>> GetUpcomingSessionsAsync(int withinDays, System.Threading.CancellationToken cancellationToken = default);
    Task AddAsync(ReviewSessionInfo session);
    Task UpdateAsync(ReviewSessionInfo session);
    Task RemoveAsync(ReviewSessionInfo session);
    Task SaveChangesAsync();
}
