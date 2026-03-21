using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly GpmsDbContext _context;
    public FeedbackRepository(GpmsDbContext context) => _context = context;

    public async Task<Feedback?> GetByIdAsync(int feedbackId) => await _context.Feedbacks.FindAsync(feedbackId);
    public async Task<Feedback?> GetByEvaluationIdAsync(int evaluationId) => await _context.Feedbacks.FirstOrDefaultAsync(f => f.EvaluationID == evaluationId);

    public async Task<IEnumerable<Feedback>> GetBySupervisorAsync(string supervisorId) =>
        await _context.Feedbacks
            .Include(f => f.FeedbackApproval)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.Group)
                    .ThenInclude(g => g.Project)
                        .ThenInclude(p => p.ProjectSupervisors)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.Reviewer)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.ReviewRound)
            .Where(f => f.FeedbackApproval != null &&
                        (f.FeedbackApproval.SupervisorID == supervisorId ||
                         f.Evaluation.Group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId)))
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    
    public async Task<IEnumerable<Feedback>> GetPendingApprovalsBySupervisorAsync(string supervisorId) =>
        await _context.Feedbacks
            .Include(f => f.FeedbackApproval)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.Group)
                    .ThenInclude(g => g.Project)
                        .ThenInclude(p => p.ProjectSupervisors)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.Reviewer)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.ReviewRound)
            .Where(f => f.FeedbackApproval != null &&
                        f.FeedbackApproval.ApprovalStatus == ApprovalStatus.Pending &&
                        (f.FeedbackApproval.SupervisorID == supervisorId ||
                         f.Evaluation.Group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId)))
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Feedback>> GetVisibleFeedbacksForStudentAsync(string studentId) =>
        await _context.Feedbacks
            .Include(f => f.FeedbackApproval)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.Group)
                    .ThenInclude(g => g.GroupMembers)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.Reviewer)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.ReviewRound)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.EvaluationDetails)
                    .ThenInclude(d => d.Item)
            .Where(f => f.FeedbackApproval != null && 
                        f.FeedbackApproval.IsVisibleToStudent &&
                        f.Evaluation.Group.GroupMembers.Any(gm => gm.UserID == studentId))
            .OrderByDescending(f => f.Evaluation.ReviewRound.StartDate)
            .ToListAsync();

    public async Task<Feedback?> GetByIdWithDetailsAsync(int feedbackId) =>
        await _context.Feedbacks
            .Include(f => f.FeedbackApproval)
                .ThenInclude(fa => fa!.Supervisor)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.Group)
                    .ThenInclude(g => g.GroupMembers)
                        .ThenInclude(m => m.User)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.Group)
                    .ThenInclude(g => g.Project)
                        .ThenInclude(p => p.ProjectSupervisors)
                            .ThenInclude(ps => ps.Lecturer)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.Reviewer)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.ReviewRound)
            .Include(f => f.Evaluation)
                .ThenInclude(e => e.EvaluationDetails)
                    .ThenInclude(d => d.Item)
            .FirstOrDefaultAsync(f => f.FeedbackID == feedbackId);

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

    public async Task<IReadOnlyList<FeedbackApproval>> GetApprovalsPendingAutoReleaseAsync(DateTime approvedBeforeUtc, CancellationToken cancellationToken = default) =>
        await _context.FeedbackApprovals
            .Where(fa => fa.ApprovalStatus == ApprovalStatus.Approved &&
                         !fa.IsVisibleToStudent &&
                         fa.ApprovedAt <= approvedBeforeUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Feedback feedback) => await _context.Feedbacks.AddAsync(feedback);
    public Task UpdateApprovalAsync(FeedbackApproval approval) { _context.FeedbackApprovals.Update(approval); return Task.CompletedTask; }
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
