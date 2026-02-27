using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IReviewerAssignmentRepository
{
    Task<IEnumerable<ReviewerAssignment>> GetByRoundAndGroupAsync(int roundId, int groupId);
    Task AddAsync(ReviewerAssignment assignment);
    Task RemoveAsync(ReviewerAssignment assignment);
    Task SaveChangesAsync();
}
