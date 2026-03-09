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
        await _context.Evaluations.Include(e => e.EvaluationDetails).FirstOrDefaultAsync(e => e.ReviewRoundID == roundId && e.ReviewerID == reviewerId && e.GroupID == groupId);
    public async Task AddAsync(Evaluation evaluation) => await _context.Evaluations.AddAsync(evaluation);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
