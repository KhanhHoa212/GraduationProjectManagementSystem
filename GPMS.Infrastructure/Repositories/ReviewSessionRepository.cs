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
            .Include(s => s.Group)
                .ThenInclude(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
            .Include(s => s.Group)
                .ThenInclude(g => g.Project)
                    .ThenInclude(p => p.ProjectSupervisors)
                        .ThenInclude(ps => ps.Lecturer)
            .Include(s => s.ReviewRound)
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

    public async Task AddAsync(ReviewSessionInfo session)
    {
        await _context.ReviewSessions.AddAsync(session);
    }

    public void Update(ReviewSessionInfo session)
    {
        _context.ReviewSessions.Update(session);
    }

    public void Delete(ReviewSessionInfo session)
    {
        _context.ReviewSessions.Remove(session);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
