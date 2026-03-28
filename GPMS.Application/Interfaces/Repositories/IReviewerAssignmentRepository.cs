using GPMS.Application.DTOs.Lecturer;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IReviewerAssignmentRepository
{
    // Entity methods (write-path — needed for evaluation submission)
    Task<ReviewerAssignment?> GetByIdAsync(int assignmentId);
    Task<IEnumerable<ReviewerAssignment>> GetByRoundAndGroupAsync(int roundId, int groupId);
    Task<IEnumerable<ReviewerAssignment>> GetByReviewerAsync(string reviewerId);
    Task AddAsync(ReviewerAssignment assignment);
    Task RemoveAsync(ReviewerAssignment assignment);
    Task SaveChangesAsync();

    // DTO Projection methods (read-only — no entity tracking)
    Task<IEnumerable<ReviewAssignmentItemDto>> GetAssignmentDtosByReviewerAsync(string reviewerId);
}
