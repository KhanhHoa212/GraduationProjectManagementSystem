using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IReviewSessionRepository
{
    Task<ReviewSessionInfo?> GetByIdAsync(int sessionId);
    Task<IEnumerable<ReviewSessionInfo>> GetByRoundIdAsync(int roundId);
    Task<IEnumerable<ReviewSessionInfo>> GetByGroupIdAsync(int groupId);
    Task AddAsync(ReviewSessionInfo session);
    void Update(ReviewSessionInfo session);
    void Delete(ReviewSessionInfo session);
    Task SaveChangesAsync();
}
