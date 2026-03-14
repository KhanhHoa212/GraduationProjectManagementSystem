using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly GpmsDbContext _context;
    public FeedbackRepository(GpmsDbContext context) => _context = context;

    public async Task<Feedback?> GetByIdAsync(int feedbackId) => await _context.Feedbacks.FindAsync(feedbackId);
    public async Task<Feedback?> GetByEvaluationIdAsync(int evaluationId) => await _context.Feedbacks.FirstOrDefaultAsync(f => f.EvaluationID == evaluationId);
    
    public async Task<IEnumerable<Feedback>> GetRecentFeedbacksByStudentAsync(string studentId, int count)
    {
        // 1. Find the student's active group
        var group = await _context.ProjectGroups
            .Include(g => g.GroupMembers)
            .Where(g => g.GroupMembers.Any(m => m.UserID == studentId))
            .FirstOrDefaultAsync();

        if (group == null)
            return Enumerable.Empty<Feedback>();

        return await _context.Feedbacks
            .Include(f => f.Evaluation)
            .ThenInclude(e => e.Reviewer)
            .Where(f => f.Evaluation.GroupID == group.GroupID)
            .OrderByDescending(f => f.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task AddAsync(Feedback feedback) => await _context.Feedbacks.AddAsync(feedback);
    public async Task UpdateApprovalAsync(FeedbackApproval approval) => _context.FeedbackApprovals.Update(approval);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
