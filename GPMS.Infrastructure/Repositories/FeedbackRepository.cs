using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly GpmsDbContext _context;
    public FeedbackRepository(GpmsDbContext context) => _context = context;

    public async Task<Feedback?> GetByIdAsync(int feedbackId) => await _context.Feedbacks.FindAsync(feedbackId);
    public async Task<Feedback?> GetByEvaluationIdAsync(int evaluationId) => await _context.Feedbacks.FirstOrDefaultAsync(f => f.EvaluationID == evaluationId);
    public async Task AddAsync(Feedback feedback) => await _context.Feedbacks.AddAsync(feedback);
    public async Task UpdateApprovalAsync(FeedbackApproval approval) => _context.FeedbackApprovals.Update(approval);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
