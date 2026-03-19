using GPMS.Application.Interfaces.Repositories;
using GPMS.Domain.Entities;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Repositories;

public class MentorRoundReviewRepository : IMentorRoundReviewRepository
{
    private readonly GpmsDbContext _context;

    public MentorRoundReviewRepository(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task<MentorRoundReview?> GetAsync(int reviewRoundId, int groupId) =>
        await _context.MentorRoundReviews
            .Include(m => m.ReviewRound)
            .Include(m => m.Group)
                .ThenInclude(g => g.Project)
            .Include(m => m.Supervisor)
            .FirstOrDefaultAsync(m => m.ReviewRoundID == reviewRoundId && m.GroupID == groupId);

    public async Task<IEnumerable<MentorRoundReview>> GetBySupervisorAsync(string supervisorId) =>
        await _context.MentorRoundReviews
            .Include(m => m.ReviewRound)
            .Include(m => m.Group)
                .ThenInclude(g => g.Project)
            .Where(m => m.SupervisorID == supervisorId)
            .ToListAsync();

    public async Task AddAsync(MentorRoundReview review) =>
        await _context.MentorRoundReviews.AddAsync(review);

    public void Update(MentorRoundReview review) =>
        _context.MentorRoundReviews.Update(review);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
