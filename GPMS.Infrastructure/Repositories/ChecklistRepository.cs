using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class ChecklistRepository : IChecklistRepository
{
    private readonly GpmsDbContext _context;
    public ChecklistRepository(GpmsDbContext context) => _context = context;

    public async Task<ReviewChecklist?> GetByRoundIdAsync(int roundId) =>
        await _context.ReviewChecklists
            .Include(c => c.ReviewRound)
            .Include(c => c.ChecklistItems)
                .ThenInclude(i => i.RubricDescriptions)
            .FirstOrDefaultAsync(c => c.ReviewRoundID == roundId);

    public async Task AddAsync(ReviewChecklist checklist) => await _context.ReviewChecklists.AddAsync(checklist);
    public void Update(ReviewChecklist checklist) => _context.ReviewChecklists.Update(checklist);
    public void Delete(ReviewChecklist checklist) => _context.ReviewChecklists.Remove(checklist);
    
    public async Task<IEnumerable<ReviewChecklist>> GetChecklistsBySemesterIdAsync(int semesterId) =>
        await _context.ReviewChecklists
            .Include(c => c.ReviewRound)
            .Include(c => c.ChecklistItems)
                .ThenInclude(i => i.RubricDescriptions)
            .Where(c => c.ReviewRound.SemesterID == semesterId)
            .ToListAsync();

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
