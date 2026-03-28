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

    public async Task<ReviewRound?> GetByIdAsync(int roundId) => 
        await _context.ReviewRounds
            .Include(r => r.Semester)
            .Include(r => r.SubmissionRequirements)
            .FirstOrDefaultAsync(r => r.ReviewRoundID == roundId);
    
    public async Task<ReviewRound?> GetByIdWithChecklistAsync(int roundId) =>
        await _context.ReviewRounds
            .Include(r => r.ReviewChecklist)
                .ThenInclude(c => c!.ChecklistItems)
            .Include(r => r.Semester)
            .FirstOrDefaultAsync(r => r.ReviewRoundID == roundId);
            
    public async Task<IEnumerable<ReviewRound>> GetBySemesterAsync(int semesterId) => 
        await _context.ReviewRounds
            .Include(r => r.Semester)
            .Include(r => r.SubmissionRequirements)
            .Where(r => r.SemesterID == semesterId)
            .ToListAsync();
            
    public async Task<IEnumerable<ReviewRound>> GetBySemesterWithRequirementsAsync(int semesterId) => 
        await _context.ReviewRounds
            .Include(r => r.SubmissionRequirements)
            .Where(r => r.SemesterID == semesterId)
            .OrderBy(r => r.RoundNumber)
            .ToListAsync();

    public async Task<IEnumerable<ReviewRound>> GetAllWithRequirementsAsync() => 
        await _context.ReviewRounds
            .Include(r => r.SubmissionRequirements)
            .OrderBy(r => r.SemesterID)
            .ThenBy(r => r.RoundNumber)
            .ToListAsync();
        
    public async Task<IEnumerable<ReviewRound>> GetUpcomingRoundsAsync(int count = 5) =>
        await _context.ReviewRounds
            .Where(r => r.EndDate >= System.DateTime.UtcNow.Date)
            .OrderBy(r => r.StartDate)
            .Take(count)
            .ToListAsync();
    public async Task AddAsync(ReviewRound round) => await _context.ReviewRounds.AddAsync(round);
    public void Update(ReviewRound round) => _context.ReviewRounds.Update(round);
    public void Delete(ReviewRound round) => _context.ReviewRounds.Remove(round);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
