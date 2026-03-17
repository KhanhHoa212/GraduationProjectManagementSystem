using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class EvaluationRepository : IEvaluationRepository
{
    private readonly GpmsDbContext _context;
    public EvaluationRepository(GpmsDbContext context) => _context = context;

    public async Task<Evaluation?> GetByIdAsync(int evaluationId) => await _context.Evaluations.FindAsync(evaluationId);
    public async Task<Evaluation?> GetByReviewerAndGroupAsync(int roundId, string reviewerId, int groupId) => 
        await _context.Evaluations.FirstOrDefaultAsync(e => e.ReviewRoundID == roundId && e.ReviewerID == reviewerId && e.GroupID == groupId);
    
    public async Task<IEnumerable<Evaluation>> GetByGroupWithDetailsAsync(int groupId) => 
        await _context.Evaluations
            .Include(e => e.ReviewRound)
            .Include(e => e.Reviewer)
            .Include(e => e.EvaluationDetails)
                .ThenInclude(d => d.Item)
            .Include(e => e.Feedback)
            .Where(e => e.GroupID == groupId)
            .ToListAsync();

    public async Task AddAsync(Evaluation evaluation) => await _context.Evaluations.AddAsync(evaluation);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
