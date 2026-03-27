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

public class SubmissionSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public int Order => 6;

    public SubmissionSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var faker = new Faker();

        var rounds = await _context.ReviewRounds
            .Include(r => r.Semester)
            .Include(r => r.SubmissionRequirements)
            .Where(r => r.SemesterID >= 5)
            .ToListAsync();

        var groups = await _context.ProjectGroups
            .Include(g => g.Project)
            .Include(g => g.GroupMembers)
            .ToListAsync();

        if (!rounds.Any() || !groups.Any()) return;

        var submissions = new List<Submission>();

        foreach (var round in rounds)
        {
            var targetGroups = groups.Where(g => g.Project.SemesterID == round.SemesterID).ToList();
            if (!targetGroups.Any() || !round.SubmissionRequirements.Any()) continue;

            foreach (var group in targetGroups)
            {
                var leader = group.GroupMembers.FirstOrDefault(m => m.RoleInGroup == GroupRole.Leader);
                if (leader == null) continue;

                var rand = faker.Random.Double();
                if (rand > 0.9 && round.Status != RoundStatus.Completed) continue; // chưa nộp

                SubmissionStatus status = rand <= 0.8 ? SubmissionStatus.OnTime : SubmissionStatus.Late;

                var baseDate = round.SubmissionDeadline > DateTime.MinValue.AddDays(7)
                    ? round.SubmissionDeadline
                    : DateTime.UtcNow.AddDays(10);

                var submittedAt = status == SubmissionStatus.OnTime
                    ? baseDate.AddDays(-faker.Random.Int(1, 3))
                    : baseDate.AddHours(faker.Random.Int(1, 24));

                foreach (var req in round.SubmissionRequirements)
                {
                    // Check if this submission already exists
                    var exists = await _context.Submissions.AnyAsync(s =>
                        s.GroupID == group.GroupID &&
                        s.RequirementID == req.RequirementID &&
                        s.Version == 1);

                    if (exists) continue;

                    var fileName = req.DocumentName.Contains("Báo cáo") ? "BaoCaoTienDo.pdf" : "SourceCode.zip";
                    submissions.Add(new Submission
                    {
                        GroupID = group.GroupID,
                        RequirementID = req.RequirementID,
                        SubmittedBy = leader.UserID,
                        SubmittedAt = submittedAt,
                        FileUrl = $"/uploads/{round.Semester.SemesterCode}/Round{round.RoundNumber}/{group.GroupID}/{fileName}",
                        FileName = fileName,
                        FileSizeMB = (decimal)faker.Random.Double(2.5, 45.0),
                        Status = status,
                        Version = 1
                    });
                }
            }
        }

        if (submissions.Any())
        {
            await _context.Submissions.AddRangeAsync(submissions);
            await _context.SaveChangesAsync();
        }
    }
}

