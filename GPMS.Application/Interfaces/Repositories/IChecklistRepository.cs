using GPMS.Domain.Entities;

namespace GPMS.Application.Interfaces.Repositories;

public interface IChecklistRepository
{
    Task<ReviewChecklist?> GetByRoundIdAsync(int roundId);
    Task AddAsync(ReviewChecklist checklist);
    void Update(ReviewChecklist checklist);
    void Delete(ReviewChecklist checklist);
    Task SaveChangesAsync();
}
