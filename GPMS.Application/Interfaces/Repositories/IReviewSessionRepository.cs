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
    Task AddAsync(ReviewSessionInfo session);
    Task UpdateAsync(ReviewSessionInfo session);
    Task RemoveAsync(ReviewSessionInfo session);
    Task SaveChangesAsync();
}
