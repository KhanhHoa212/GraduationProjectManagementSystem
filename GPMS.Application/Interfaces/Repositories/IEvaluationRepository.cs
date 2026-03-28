using GPMS.Application.DTOs.Lecturer;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IEvaluationRepository
{
    // Entity methods (write-path — needed for evaluation submission/update)
    Task<Evaluation?> GetByIdAsync(int evaluationId);
    Task<Evaluation?> GetByReviewerAndGroupAsync(int roundId, string reviewerId, int groupId);
    Task<IEnumerable<Evaluation>> GetByGroupWithDetailsAsync(int groupId);
    Task<IEnumerable<Evaluation>> GetSubmittedByReviewerAsync(string reviewerId);
    Task AddAsync(Evaluation evaluation);
    void Update(Evaluation evaluation);
    Task SaveChangesAsync();

    // DTO Projection methods (read-only — no entity tracking)
    Task<IEnumerable<LecturerReviewHistoryItemDto>> GetHistoryDtosByReviewerAsync(string reviewerId);
}
