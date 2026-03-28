using GPMS.Application.DTOs.Lecturer;
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

    // ─── Entity methods (write-path) ───────────────────────────────────────────

    public async Task<Feedback?> GetByIdAsync(int feedbackId) => await _context.Feedbacks.FindAsync(feedbackId);
    public async Task<Feedback?> GetByEvaluationIdAsync(int evaluationId) => await _context.Feedbacks.FirstOrDefaultAsync(f => f.EvaluationID == evaluationId);

    public async Task<IEnumerable<Feedback>> GetBySupervisorAsync(string supervisorId) =>
        await _context.Feedbacks
            .AsSplitQuery()
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
            .OrderByDescending(f => f.FeedbackApproval!.ApprovedAt ?? f.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Feedback>> GetPendingApprovalsBySupervisorAsync(string supervisorId) =>
        await _context.Feedbacks
            .AsSplitQuery()
            .Include(f => f.FeedbackApproval)
                .ThenInclude(fa => fa!.Supervisor)
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

    public async Task<Feedback?> GetByIdWithDetailsAsync(int feedbackId) =>
        await _context.Feedbacks
            .AsSplitQuery()
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

    public async Task<IEnumerable<Feedback>> GetVisibleFeedbacksForStudentAsync(string studentId) =>
        await _context.Feedbacks
            .AsSplitQuery()
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

    public async Task<IEnumerable<Feedback>> GetRecentFeedbacksByStudentAsync(string studentId, int count)
    {
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

    // ─── DTO Projection methods (read-only) ────────────────────────────────────

    public async Task<IEnumerable<PendingFeedbackItemDto>> GetPendingApprovalDtosBySupervisorAsync(string supervisorId) =>
        await _context.Feedbacks
            .Where(f => f.FeedbackApproval != null &&
                        f.FeedbackApproval.ApprovalStatus == ApprovalStatus.Pending &&
                        (f.FeedbackApproval.SupervisorID == supervisorId ||
                         f.Evaluation.Group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId)))
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new PendingFeedbackItemDto
            {
                FeedbackId = f.FeedbackID,
                EvaluationId = f.EvaluationID,
                GroupId = f.Evaluation.GroupID,
                GroupName = f.Evaluation.Group.GroupName,
                ProjectName = f.Evaluation.Group.Project.ProjectName,
                ReviewRoundName = f.Evaluation.ReviewRound.RoundNumber.ToString(),
                RoundNumber = f.Evaluation.ReviewRound.RoundNumber,
                ReviewerName = f.Evaluation.Reviewer.FullName,
                SubmittedAt = f.Evaluation.SubmittedAt ?? f.CreatedAt,
                CreatedAt = f.CreatedAt,
                AutoReleaseAt = f.FeedbackApproval != null && f.FeedbackApproval.ApprovedAt != null
                    ? f.FeedbackApproval.ApprovedAt.Value.AddDays(7)
                    : (DateTime?)null,
                ApprovalStatus = f.FeedbackApproval != null
                    ? f.FeedbackApproval.ApprovalStatus
                    : ApprovalStatus.Pending
            })
            .ToListAsync();

    public async Task<LecturerFeedbackApprovalDetailDto?> GetApprovalDetailDtoAsync(int feedbackId) =>
        await _context.Feedbacks
            .Where(f => f.FeedbackID == feedbackId)
            .Select(f => new LecturerFeedbackApprovalDetailDto
            {
                FeedbackId = f.FeedbackID,
                EvaluationId = f.EvaluationID,
                GroupName = f.Evaluation.Group.GroupName,
                GroupId = f.Evaluation.GroupID,
                ReviewRoundName = f.Evaluation.ReviewRound.RoundNumber.ToString(),
                CurrentRoundIndex = f.Evaluation.ReviewRound.RoundNumber,
                TotalRounds = 0, // populated in service from roundRepo
                SubmittedAt = f.Evaluation.SubmittedAt ?? f.CreatedAt,
                ApprovalStatus = f.FeedbackApproval != null
                    ? f.FeedbackApproval.ApprovalStatus
                    : ApprovalStatus.Pending,
                SupervisorComment = f.FeedbackApproval != null ? f.FeedbackApproval.SupervisorComment : null,
                MentorGateStatus = MentorGateStatus.Pending, // populated in service from mentorRepo
                MentorGateComment = null,
                ReviewerName = f.Evaluation.Reviewer.FullName,
                FeedbackContent = f.Content,
                Scores = f.Evaluation.EvaluationDetails
                    .OrderBy(d => d.Item.OrderIndex)
                    .Select(d => new EvaluationScoreItemDto
                    {
                        ItemId = d.ItemID,
                        ItemCode = d.Item.ItemCode,
                        ItemName = d.Item.ItemName,
                        ItemContent = d.Item.ItemContent,
                        Section = d.Item.Section,
                        ItemType = d.Item.ItemType,
                        Assessment = d.Assessment,
                        ReviewerComment = d.Comment,
                        MentorComment = d.MentorComment,
                        GradeDescription = d.GradeDescription,
                        RubricDescriptions = d.Item.RubricDescriptions
                            .Select(r => new RubricDescriptionDto
                            {
                                GradeLevel = r.GradeLevel,
                                Description = r.Description
                            }).ToList()
                    }).ToList(),
                Members = f.Evaluation.Group.GroupMembers
                    .Select(m => new StudentMemberDto
                    {
                        UserId = m.UserID,
                        FullName = m.User.FullName,
                        Email = m.User.Email,
                        RoleInGroup = m.RoleInGroup.ToString(),
                        AvatarUrl = "https://ui-avatars.com/api/?name=" +
                                    Uri.EscapeDataString(m.User.FullName ?? "U") +
                                    "&background=E5E7EB&color=374151"
                    }).ToList()
            })
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<LecturerFeedbackHistoryItemDto>> GetHistoryDtosBySupervisorAsync(string supervisorId) =>
        await _context.Feedbacks
            .Where(f => f.FeedbackApproval != null &&
                        (f.FeedbackApproval.SupervisorID == supervisorId ||
                         f.Evaluation.Group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId)))
            .OrderByDescending(f => f.FeedbackApproval!.ApprovedAt ?? f.CreatedAt)
            .Select(f => new LecturerFeedbackHistoryItemDto
            {
                FeedbackId = f.FeedbackID,
                GroupId = f.Evaluation.GroupID,
                GroupName = f.Evaluation.Group.GroupName,
                ProjectName = f.Evaluation.Group.Project.ProjectName,
                ReviewerName = f.Evaluation.Reviewer.FullName,
                RoundNumber = f.Evaluation.ReviewRound.RoundNumber,
                ApprovalStatus = f.FeedbackApproval != null
                    ? f.FeedbackApproval.ApprovalStatus
                    : ApprovalStatus.Pending,
                UpdatedAt = f.FeedbackApproval != null && f.FeedbackApproval.ApprovedAt != null
                    ? f.FeedbackApproval.ApprovedAt.Value
                    : f.CreatedAt,
                IsVisibleToStudent = f.FeedbackApproval != null && f.FeedbackApproval.IsVisibleToStudent,
                SupervisorComment = f.FeedbackApproval != null ? f.FeedbackApproval.SupervisorComment : null
            })
            .ToListAsync();
}
