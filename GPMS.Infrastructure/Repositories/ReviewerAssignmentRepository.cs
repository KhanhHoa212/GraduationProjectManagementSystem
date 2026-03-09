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

    public async Task<IEnumerable<ReviewerAssignment>> GetByRoundAndGroupAsync(int roundId, int groupId) => 
        await _context.ReviewerAssignments.Where(ra => ra.ReviewRoundID == roundId && ra.GroupID == groupId).ToListAsync();

    public async Task<IEnumerable<ReviewerAssignment>> GetByReviewerAsync(string reviewerId) =>
        await _context.ReviewerAssignments
            .Include(ra => ra.ReviewRound)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.Project)
            .Include(ra => ra.Group)
                .ThenInclude(g => g.ReviewSessions)
                    .ThenInclude(rs => rs.Room)
            .Where(ra => ra.ReviewerID == reviewerId)
            .OrderByDescending(ra => ra.ReviewRound.StartDate)
            .ToListAsync();

    public async Task AddAsync(ReviewerAssignment assignment) => await _context.ReviewerAssignments.AddAsync(assignment);
    public async Task RemoveAsync(ReviewerAssignment assignment) => _context.ReviewerAssignments.Remove(assignment);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
