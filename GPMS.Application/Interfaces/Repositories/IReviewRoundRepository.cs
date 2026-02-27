using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IReviewRoundRepository
{
    Task<ReviewRound?> GetByIdAsync(int roundId);
    Task<IEnumerable<ReviewRound>> GetBySemesterAsync(int semesterId);
    Task AddAsync(ReviewRound round);
    Task SaveChangesAsync();
}
