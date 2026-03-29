using GPMS.Application.Interfaces.Repositories;
using GPMS.Domain.Entities;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class ReviewSessionRepository : IReviewSessionRepository
{
    private readonly GpmsDbContext _context;
    public ReviewSessionRepository(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewSessionInfo?> GetByIdAsync(int sessionId)
    {
        return await _context.ReviewSessions
            .Include(s => s.Committee)
            .Include(s => s.Room)
            .Include(s => s.ReviewRound)
            .Include(s => s.Group)
                .ThenInclude(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
            .Include(s => s.Group)
                .ThenInclude(g => g.Project)
                    .ThenInclude(p => p.ProjectSupervisors)
                        .ThenInclude(ps => ps.Lecturer)
            .FirstOrDefaultAsync(s => s.SessionID == sessionId);
    }

    public async Task<IEnumerable<ReviewSessionInfo>> GetByRoundIdAsync(int roundId)
    {
        return await _context.ReviewSessions
            .Include(s => s.Room)
            .Include(s => s.Group)
            .Where(s => s.ReviewRoundID == roundId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReviewSessionInfo>> GetByGroupIdAsync(int groupId)
    {
        return await _context.ReviewSessions
            .Include(s => s.Room)
            .Include(s => s.ReviewRound)
            .Where(s => s.GroupID == groupId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReviewSessionInfo>> GetByRoundAndDateAsync(int roundId, DateTime date) =>
        await _context.ReviewSessions
            .Include(s => s.Room)
            .Include(s => s.Group)
                .ThenInclude(g => g.Project)
                    .ThenInclude(p => p.ProjectSupervisors)
                        .ThenInclude(ps => ps.Lecturer)
            .Include(s => s.Group)
                .ThenInclude(g => g.ReviewerAssignments)
                    .ThenInclude(ra => ra.Reviewer)
            .Where(s => s.ReviewRoundID == roundId && s.ScheduledAt.Date == date.Date)
            .ToListAsync();

    public async Task<IEnumerable<ReviewSessionInfo>> GetByDateAsync(DateTime date) =>
        await _context.ReviewSessions
            .Include(s => s.Committee)
            .Where(s => s.ScheduledAt.Date == date.Date)
            .ToListAsync();

    public async Task<IEnumerable<ReviewSessionInfo>> GetByRoundAsync(int roundId) =>
        await _context.ReviewSessions
            .Where(s => s.ReviewRoundID == roundId)
            .ToListAsync();

    public async Task<ReviewSessionInfo?> GetByRoundAndGroupAsync(int roundId, int groupId) =>
        await _context.ReviewSessions
            .FirstOrDefaultAsync(s => s.ReviewRoundID == roundId && s.GroupID == groupId);

    public async Task AddAsync(ReviewSessionInfo session)
    {
        await _context.ReviewSessions.AddAsync(session);
    }

    public Task UpdateAsync(ReviewSessionInfo session)
    {
        _context.ReviewSessions.Update(session);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(ReviewSessionInfo session)
    {
        _context.ReviewSessions.Remove(session);
        return Task.CompletedTask;
    }

<<<<<<< HEAD
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
=======
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<List<ReviewSessionInfo>> GetUpcomingSessionsAsync(int withinDays, System.Threading.CancellationToken cancellationToken = default)
    {
        var today = System.DateTime.UtcNow.Date;
        return await _context.ReviewSessions
            .Include(s => s.ReviewRound)
            .Include(s => s.Group)
                .ThenInclude(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
            .Where(s => s.ScheduledAt.Date >= today && s.ScheduledAt.Date <= today.AddDays(withinDays))
            .ToListAsync(cancellationToken);
>>>>>>> d8f6ae032d950c7c5ac9eb8a9c9c9131baab0cbf
    }
}
