using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IReviewSessionRepository
{
    Task<ReviewSessionInfo?> GetByIdAsync(int sessionId);
    Task<IEnumerable<ReviewSessionInfo>> GetByRoundIdAsync(int roundId);
    Task<IEnumerable<ReviewSessionInfo>> GetByGroupIdAsync(int groupId);
    Task<IEnumerable<ReviewSessionInfo>> GetByRoundAndDateAsync(int roundId, DateTime date);
    Task<IEnumerable<ReviewSessionInfo>> GetByDateAsync(DateTime date);
    Task<IEnumerable<ReviewSessionInfo>> GetByRoundAsync(int roundId);
    Task<ReviewSessionInfo?> GetByRoundAndGroupAsync(int roundId, int groupId);
<<<<<<< HEAD
=======
    Task<ReviewSessionInfo?> GetByIdAsync(int sessionId);
    Task<List<ReviewSessionInfo>> GetUpcomingSessionsAsync(int withinDays, System.Threading.CancellationToken cancellationToken = default);
>>>>>>> d8f6ae032d950c7c5ac9eb8a9c9c9131baab0cbf
    Task AddAsync(ReviewSessionInfo session);
    Task UpdateAsync(ReviewSessionInfo session);
    Task RemoveAsync(ReviewSessionInfo session);
    Task SaveChangesAsync();
}
