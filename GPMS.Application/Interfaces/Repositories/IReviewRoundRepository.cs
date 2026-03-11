using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IReviewRoundRepository
{
    Task<ReviewRound?> GetByIdAsync(int roundId);
    Task<IEnumerable<ReviewRound>> GetBySemesterAsync(int semesterId);
    Task AddAsync(ReviewRound round);
    void Update(ReviewRound round);
    void Delete(ReviewRound round);
    Task SaveChangesAsync();
}
