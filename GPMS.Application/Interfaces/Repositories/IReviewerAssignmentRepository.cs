using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IReviewerAssignmentRepository
{
    Task<ReviewerAssignment?> GetByIdAsync(int assignmentId);
    Task<IEnumerable<ReviewerAssignment>> GetByRoundAndGroupAsync(int roundId, int groupId);
    Task<IEnumerable<ReviewerAssignment>> GetByReviewerAsync(string reviewerId);
    Task AddAsync(ReviewerAssignment assignment);
    Task RemoveAsync(ReviewerAssignment assignment);
    Task SaveChangesAsync();
}
