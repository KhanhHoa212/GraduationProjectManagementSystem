using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        // Check for specific seeded submissions
        if (await _context.Submissions.AnyAsync(s => s.FileUrl.Contains("/uploads/SP25/"))) return;

        var round1 = await _context.ReviewRounds
            .FirstOrDefaultAsync(r => r.RoundNumber == 1 && r.SemesterID == 1);
        if (round1 == null) return;

        var requirements = await _context.SubmissionRequirements
            .Where(sr => sr.ReviewRoundID == round1.ReviewRoundID)
            .ToListAsync();
        
        var groups = await _context.ProjectGroups.Where(g => g.GroupName.StartsWith("Team ")).ToListAsync();

        int groupCount = 0;
        foreach (var @group in groups)
        {
            var leaderMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupID == @group.GroupID && gm.RoleInGroup == GroupRole.Leader);
            
            if (leaderMember == null) continue;

            // Status: 8 OnTime, 2 Late
            var status = groupCount < 8 ? SubmissionStatus.OnTime : SubmissionStatus.Late;
            var submittedAt = status == SubmissionStatus.OnTime 
                ? round1.SubmissionDeadline.AddDays(-2) 
                : round1.SubmissionDeadline.AddHours(5);

            foreach (var req in requirements)
            {
                var fileName = req.DocumentName.Contains("Báo cáo") ? "BaoCaoTienDo.pdf" : "SourceCode.zip";
                _context.Submissions.Add(new Submission
                {
                    GroupID = @group.GroupID,
                    RequirementID = req.RequirementID,
                    SubmittedBy = leaderMember.UserID,
                    SubmittedAt = submittedAt,
                    FileUrl = $"/uploads/SP25/Round1/{@group.GroupID}/{fileName}",
                    FileName = fileName,
                    FileSizeMB = 5,
                    Status = status,
                    Version = 1
                });
            }
            groupCount++;
        }

        await _context.SaveChangesAsync();
    }
}
