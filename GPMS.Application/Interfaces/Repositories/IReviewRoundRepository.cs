using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IReviewRoundRepository
{
    Task<ReviewRound?> GetByIdAsync(int roundId);
    Task<ReviewRound?> GetByIdWithChecklistAsync(int roundId);
    Task<IEnumerable<ReviewRound>> GetBySemesterAsync(int semesterId);
    Task<IEnumerable<ReviewRound>> GetBySemesterWithRequirementsAsync(int semesterId);
    Task<IEnumerable<ReviewRound>> GetAllWithRequirementsAsync();
    Task<IEnumerable<ReviewRound>> GetUpcomingRoundsAsync(int count = 5);
    Task AddAsync(ReviewRound round);
    void Update(ReviewRound round);
    void Delete(ReviewRound round);
    Task SaveChangesAsync();
}
