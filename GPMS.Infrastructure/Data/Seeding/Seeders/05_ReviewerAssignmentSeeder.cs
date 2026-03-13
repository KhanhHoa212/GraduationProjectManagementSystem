using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Data.Seeding.Seeders;

public class ReviewerAssignmentSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public int Order => 5;

    public ReviewerAssignmentSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if assignments exist for potential seeded rounds (Round 1 or 2)
        if (await _context.ReviewerAssignments.AnyAsync(ra => ra.ReviewRound.SemesterID == 1)) return;

        var groups = await _context.ProjectGroups.Where(g => g.GroupName.StartsWith("Team ")).ToListAsync();
        var rounds = await _context.ReviewRounds
            .Where(r => r.SemesterID == 1 && (r.Status == RoundStatus.Completed || r.Status == RoundStatus.Ongoing))
            .ToListAsync();
        var lecturers = await _context.UserRoles
            .Where(r => r.RoleName == RoleName.Lecturer)
            .Select(r => r.UserID)
            .ToListAsync();

        if (!groups.Any() || !rounds.Any() || !lecturers.Any()) return;

        foreach (var round in rounds)
        {
            var reviewerIdx = 0;
            foreach (var @group in groups)
            {
                // Get supervisor of this project to avoid assigning as reviewer
                var supervisorId = await _context.ProjectSupervisors
                    .Where(ps => ps.ProjectID == @group.ProjectID && ps.Role == ProjectRole.Main)
                    .Select(ps => ps.LecturerID)
                    .FirstOrDefaultAsync();

                // Select a lecturer who is NOT the supervisor
                string reviewerId;
                int attempts = 0;
                do
                {
                    reviewerId = lecturers[reviewerIdx % lecturers.Count];
                    reviewerIdx++;
                    attempts++;
                } while (reviewerId == supervisorId && attempts < lecturers.Count);

                _context.ReviewerAssignments.Add(new ReviewerAssignment
                {
                    ReviewRoundID = round.ReviewRoundID,
                    GroupID = @group.GroupID,
                    ReviewerID = reviewerId,
                    AssignedBy = "ADMIN001",
                    AssignedAt = DateTime.UtcNow,
                    IsRandom = true
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
