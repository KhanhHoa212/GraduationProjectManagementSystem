using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IEvaluationRepository
{
    Task<Evaluation?> GetByIdAsync(int evaluationId);
    Task<Evaluation?> GetByReviewerAndGroupAsync(int roundId, string reviewerId, int groupId);
    Task<IEnumerable<Evaluation>> GetByGroupWithDetailsAsync(int groupId);
    Task AddAsync(Evaluation evaluation);
    Task SaveChangesAsync();
}
