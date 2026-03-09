using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class ReviewRoundRepository : IReviewRoundRepository
{
    private readonly GpmsDbContext _context;
    public ReviewRoundRepository(GpmsDbContext context) => _context = context;

    public async Task<ReviewRound?> GetByIdAsync(int roundId) => await _context.ReviewRounds.FindAsync(roundId);
    
    public async Task<ReviewRound?> GetByIdWithChecklistAsync(int roundId) =>
        await _context.ReviewRounds
            .Include(r => r.ReviewChecklist)
                .ThenInclude(c => c.ChecklistItems)
            .Include(r => r.Semester)
            .FirstOrDefaultAsync(r => r.ReviewRoundID == roundId);
            
    public async Task<IEnumerable<ReviewRound>> GetBySemesterAsync(int semesterId) => 
        await _context.ReviewRounds.Where(r => r.SemesterID == semesterId).ToListAsync();
    public async Task AddAsync(ReviewRound round) => await _context.ReviewRounds.AddAsync(round);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
