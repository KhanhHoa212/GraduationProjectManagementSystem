using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Bogus;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
        if (await _context.ReviewerAssignments.CountAsync() > 20) return;

        var faker = new Faker("vi");

        var groups = await _context.ProjectGroups.Include(g => g.Project).ToListAsync();
        var rounds = await _context.ReviewRounds
            .Where(r => r.SemesterID >= 5)
            .ToListAsync();
            
        var lecturers = await _context.Users
            .Where(u => u.UserID.StartsWith("GV"))
            .Select(u => u.UserID)
            .ToListAsync();

        if (!groups.Any() || !rounds.Any() || !lecturers.Any()) return;

        var assignments = new List<ReviewerAssignment>();
        var progresses = new List<GroupRoundProgress>();

        foreach (var round in rounds)
        {
            // Chỉ assign cho các group thuộc cùng kỳ với round
            var targetGroups = groups.Where(g => g.Project.SemesterID == round.SemesterID).ToList();

            foreach (var group in targetGroups)
            {
                var supervisorId = await _context.ProjectSupervisors
                    .Where(ps => ps.ProjectID == group.ProjectID && ps.Role == ProjectRole.Main)
                    .Select(ps => ps.LecturerID)
                    .FirstOrDefaultAsync();

                var pool = lecturers.Where(l => l != supervisorId).ToList();
                if (!pool.Any()) pool = lecturers; // Fallback

                // 2 reviewers per group
                var pickedReviewers = faker.PickRandom(pool, 2).ToList();

                foreach (var revId in pickedReviewers)
                {
                    assignments.Add(new ReviewerAssignment
                    {
                        ReviewRoundID = round.ReviewRoundID,
                        GroupID = group.GroupID,
                        ReviewerID = revId,
                        AssignedBy = "ADMIN001",
                        AssignedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 20)),
                        IsRandom = true
                    });
                }

                // Random mentor decision
                MentorDecision decision;
                if (round.Status == RoundStatus.Planned)
                    decision = MentorDecision.Pending;
                else
                    decision = faker.Random.Double() > 0.05 ? MentorDecision.Accepted : MentorDecision.Rejected;

                progresses.Add(new GroupRoundProgress
                {
                    GroupID = group.GroupID,
                    ReviewRoundID = round.ReviewRoundID,
                    MentorDecision = decision,
                    MentorComment = decision == MentorDecision.Accepted ? "Nhóm làm tốt, cho bảo vệ." : "Tiến độ chậm, cần nỗ lực hơn.",
                    UpdatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 20))
                });
            }
        }

        await _context.ReviewerAssignments.AddRangeAsync(assignments);
        await _context.GroupRoundProgresses.AddRangeAsync(progresses);
        await _context.SaveChangesAsync();
    }
}

