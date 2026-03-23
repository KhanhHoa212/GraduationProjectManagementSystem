using GPMS.Domain.Entities;
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

    public async Task<ReviewerAssignment?> GetByIdAsync(int assignmentId) =>
        await _context.ReviewerAssignments
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
}
