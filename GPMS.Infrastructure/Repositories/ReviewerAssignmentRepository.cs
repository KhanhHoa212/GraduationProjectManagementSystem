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

public class ReviewerAssignmentRepository : IReviewerAssignmentRepository
{
    private readonly GpmsDbContext _context;
    public ReviewerAssignmentRepository(GpmsDbContext context) => _context = context;

    // ─── Entity methods (write-path) ───────────────────────────────────────────

    public async Task<ReviewerAssignment?> GetByIdAsync(int assignmentId) =>
        await _context.ReviewerAssignments
            .AsSplitQuery()
            .Include(ra => ra.ReviewRound)
                .ThenInclude(rr => rr.ReviewChecklist)
                    .ThenInclude(rc => rc!.ChecklistItems)
                        .ThenInclude(ci => ci.RubricDescriptions)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.Project)
                    .ThenInclude(p => p.ProjectSupervisors)
                        .ThenInclude(ps => ps.Lecturer)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.Submissions)
                    .ThenInclude(s => s.Requirement)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.MentorRoundReviews)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.ReviewSessions)
                    .ThenInclude(rs => rs.Room)
            .FirstOrDefaultAsync(ra => ra.AssignmentID == assignmentId);

    public async Task<IEnumerable<ReviewerAssignment>> GetByRoundAndGroupAsync(int roundId, int groupId) =>
        await _context.ReviewerAssignments.Where(ra => ra.ReviewRoundID == roundId && ra.GroupID == groupId).ToListAsync();

    public async Task<IEnumerable<ReviewerAssignment>> GetByReviewerAsync(string reviewerId) =>
        await _context.ReviewerAssignments
            .AsSplitQuery()
            .Include(ra => ra.ReviewRound)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.Project)
                    .ThenInclude(p => p.ProjectSupervisors)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.MentorRoundReviews)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.ReviewSessions)
                    .ThenInclude(rs => rs.Room)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.Evaluations)
                    .ThenInclude(e => e.Feedback)
            .Where(ra => ra.ReviewerID == reviewerId && !ra.Group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == reviewerId))
            .OrderByDescending(ra => ra.ReviewRound.StartDate)
            .ToListAsync();

    public async Task AddAsync(ReviewerAssignment assignment) => await _context.ReviewerAssignments.AddAsync(assignment);
    public Task RemoveAsync(ReviewerAssignment assignment) { _context.ReviewerAssignments.Remove(assignment); return Task.CompletedTask; }
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    // ─── DTO Projection methods (read-only) ────────────────────────────────────

    public async Task<IEnumerable<ReviewAssignmentItemDto>> GetAssignmentDtosByReviewerAsync(string reviewerId) =>
        await _context.ReviewerAssignments
            .Where(ra => ra.ReviewerID == reviewerId &&
                         !ra.Group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == reviewerId))
            .OrderByDescending(ra => ra.ReviewRound.StartDate)
            .Select(ra => new ReviewAssignmentItemDto
            {
                AssignmentId = ra.AssignmentID,
                GroupId = ra.GroupID,
                GroupName = ra.Group.GroupName,
                ProjectName = ra.Group.Project.ProjectName,
                ReviewRoundName = ra.ReviewRound.RoundNumber.ToString(),
                RoundNumber = ra.ReviewRound.RoundNumber,
                RoundType = ra.ReviewRound.RoundType.ToString(),
                ScheduledAt = ra.Group.ReviewSessions
                    .Where(rs => rs.ReviewRoundID == ra.ReviewRoundID)
                    .Select(rs => (DateTime?)rs.ScheduledAt)
                    .FirstOrDefault(),
                Location = ra.Group.ReviewSessions
                    .Where(rs => rs.ReviewRoundID == ra.ReviewRoundID)
                    .Select(rs => rs.MeetLink != null && rs.MeetLink != ""
                        ? "Online meeting"
                        : rs.Room != null
                            ? (rs.Room.Building != null && rs.Room.Building != ""
                                ? rs.Room.RoomCode + " - " + rs.Room.Building
                                : rs.Room.RoomCode)
                            : "Location pending")
                    .FirstOrDefault() ?? "Location pending",
                MeetLink = ra.Group.ReviewSessions
                    .Where(rs => rs.ReviewRoundID == ra.ReviewRoundID)
                    .Select(rs => rs.MeetLink)
                    .FirstOrDefault(),
                IsOnline = ra.Group.ReviewSessions
                    .Where(rs => rs.ReviewRoundID == ra.ReviewRoundID)
                    .Select(rs => rs.MeetLink != null && rs.MeetLink != "")
                    .FirstOrDefault(),
                HasEvaluation = ra.Group.Evaluations
                    .Any(e => e.ReviewRoundID == ra.ReviewRoundID &&
                              e.ReviewerID == reviewerId &&
                              e.Status == EvaluationStatus.Submitted &&
                              (e.Feedback == null ||
                               e.Feedback.FeedbackApproval == null ||
                               e.Feedback.FeedbackApproval.ApprovalStatus != ApprovalStatus.Rejected)),
                EvaluationId = ra.Group.Evaluations
                    .Where(e => e.ReviewRoundID == ra.ReviewRoundID && e.ReviewerID == reviewerId)
                    .Select(e => (int?)e.EvaluationID)
                    .FirstOrDefault(),
                StatusNote = ra.Group.MentorRoundReviews
                    .Where(m => m.ReviewRoundID == ra.ReviewRoundID)
                    .Select(m => m.DecisionStatus == MentorGateStatus.Pending ? "Waiting for Mentor Approval"
                        : m.DecisionStatus == MentorGateStatus.Rejected ? "Blocked by Mentor"
                        : (string?)null)
                    .FirstOrDefault() ??
                    (ra.Group.Evaluations
                        .Where(e => e.ReviewRoundID == ra.ReviewRoundID && e.ReviewerID == reviewerId)
                        .Any(e => e.Feedback != null &&
                                  e.Feedback.FeedbackApproval != null &&
                                  e.Feedback.FeedbackApproval.ApprovalStatus == ApprovalStatus.Rejected)
                        ? "Needs Revision"
                        : null)
            })
            .ToListAsync();
}
