using GPMS.Application.DTOs.Lecturer;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
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

    // ─── Entity methods (write-path) ───────────────────────────────────────────

    public async Task<Evaluation?> GetByIdAsync(int evaluationId) => await _context.Evaluations.FindAsync(evaluationId);

    public async Task<Evaluation?> GetByReviewerAndGroupAsync(int roundId, string reviewerId, int groupId) =>
        await _context.Evaluations
            .AsSplitQuery()
            .Include(e => e.EvaluationDetails)
                .ThenInclude(d => d.Item)
            .Include(e => e.Group)
                .ThenInclude(g => g.Project)
                    .ThenInclude(p => p.ProjectSupervisors)
                        .ThenInclude(ps => ps.Lecturer)
            .Include(e => e.Group)
                .ThenInclude(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
            .Include(e => e.ReviewRound)
                .ThenInclude(rr => rr.ReviewChecklist)
                    .ThenInclude(rc => rc!.ChecklistItems)
            .Include(e => e.Feedback)
                .ThenInclude(f => f!.FeedbackApproval)
            .FirstOrDefaultAsync(e => e.ReviewRoundID == roundId && e.ReviewerID == reviewerId && e.GroupID == groupId);

    public async Task<IEnumerable<Evaluation>> GetByGroupWithDetailsAsync(int groupId) =>
        await _context.Evaluations
            .AsSplitQuery()
            .Include(e => e.ReviewRound)
            .Include(e => e.Reviewer)
            .Include(e => e.EvaluationDetails)
                .ThenInclude(d => d.Item)
            .Include(e => e.Feedback)
            .Where(e => e.GroupID == groupId)
            .ToListAsync();

    public async Task<IEnumerable<Evaluation>> GetSubmittedByReviewerAsync(string reviewerId) =>
        await _context.Evaluations
            .Include(e => e.Group)
                .ThenInclude(g => g.Project)
            .Include(e => e.ReviewRound)
            .Include(e => e.Feedback)
                .ThenInclude(f => f!.FeedbackApproval)
            .Where(e => e.ReviewerID == reviewerId && e.SubmittedAt != null)
            .OrderByDescending(e => e.SubmittedAt)
            .ToListAsync();

    public async Task AddAsync(Evaluation evaluation) => await _context.Evaluations.AddAsync(evaluation);
    public void Update(Evaluation evaluation) => _context.Evaluations.Update(evaluation);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    // ─── DTO Projection methods (read-only) ────────────────────────────────────

    public async Task<IEnumerable<LecturerReviewHistoryItemDto>> GetHistoryDtosByReviewerAsync(string reviewerId) =>
        await _context.Evaluations
            .Where(e => e.ReviewerID == reviewerId && e.SubmittedAt != null)
            .OrderByDescending(e => e.SubmittedAt)
            .Select(e => new LecturerReviewHistoryItemDto
            {
                EvaluationId = e.EvaluationID,
                GroupId = e.GroupID,
                GroupName = e.Group.GroupName,
                ProjectName = e.Group.Project.ProjectName,
                RoundNumber = e.ReviewRound.RoundNumber,
                RoundType = e.ReviewRound.RoundType.ToString(),
                SubmittedAt = e.SubmittedAt!.Value,
                ApprovalStatus = e.Feedback != null && e.Feedback.FeedbackApproval != null
                    ? e.Feedback.FeedbackApproval.ApprovalStatus
                    : ApprovalStatus.Pending,
                FeedbackPreview = e.Feedback != null && e.Feedback.Content != null
                    ? (e.Feedback.Content.Length <= 100
                        ? e.Feedback.Content
                        : e.Feedback.Content.Substring(0, 100).TrimEnd() + "...")
                    : string.Empty
            })
            .ToListAsync();
}
