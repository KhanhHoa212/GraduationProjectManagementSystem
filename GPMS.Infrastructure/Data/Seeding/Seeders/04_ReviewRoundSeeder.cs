using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Data.Seeding.Seeders;

public class ReviewRoundSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public int Order => 4;

    public ReviewRoundSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.ReviewRounds.AnyAsync(r => r.SemesterID == 1)) return;

        var rounds = new List<ReviewRound>
        {
            new ReviewRound
            {
                SemesterID = 1,
                RoundNumber = 1,
                RoundType = RoundType.Online,
                Status = RoundStatus.Completed,
                StartDate = new DateTime(2025, 1, 15),
                EndDate = new DateTime(2025, 2, 15),
                SubmissionDeadline = new DateTime(2025, 2, 10)
            },
            new ReviewRound
            {
                SemesterID = 1,
                RoundNumber = 2,
                RoundType = RoundType.Offline,
                Status = RoundStatus.Ongoing,
                StartDate = new DateTime(2025, 2, 20),
                EndDate = new DateTime(2025, 3, 20),
                SubmissionDeadline = new DateTime(2025, 3, 15)
            },
            new ReviewRound
            {
                SemesterID = 1,
                RoundNumber = 3,
                RoundType = RoundType.Offline,
                Status = RoundStatus.Planned,
                StartDate = new DateTime(2025, 4, 1),
                EndDate = new DateTime(2025, 4, 20),
                SubmissionDeadline = new DateTime(2025, 4, 15)
            }
        };

        _context.ReviewRounds.AddRange(rounds);
        await _context.SaveChangesAsync();

        foreach (var round in rounds)
        {
            // Checklist
            var checklist = new ReviewChecklist
            {
                ReviewRoundID = round.ReviewRoundID,
                Title = $"Checklist for Round {round.RoundNumber}",
                Description = $"Detailed evaluation criteria for Review Round {round.RoundNumber}",
                CreatedBy = "GV001",
                CreatedAt = DateTime.UtcNow
            };
            _context.ReviewChecklists.Add(checklist);
            await _context.SaveChangesAsync();

            // Items
            var items = new List<ChecklistItem>
            {
                new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = $"R{round.RoundNumber}C1", ItemContent = "Tiến độ", MaxScore = 10, OrderIndex = 1 },
                new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = $"R{round.RoundNumber}C2", ItemContent = "Kỹ thuật", MaxScore = 10, OrderIndex = 2 },
                new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = $"R{round.RoundNumber}C3", ItemContent = "Trình bày", MaxScore = 5, OrderIndex = 3 },
                new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = $"R{round.RoundNumber}C4", ItemContent = "Tài liệu", MaxScore = 5, OrderIndex = 4 }
            };

            if (round.RoundNumber >= 2)
            {
                items.Add(new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = $"R{round.RoundNumber}C5", ItemContent = "Demo", MaxScore = 10, OrderIndex = 5 });
            }
            if (round.RoundNumber == 3)
            {
                items.Add(new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = $"R{round.RoundNumber}C6", ItemContent = "Phản biện", MaxScore = 10, OrderIndex = 6 });
            }

            _context.ChecklistItems.AddRange(items);

            // Submission Requirements
            _context.SubmissionRequirements.AddRange(new List<SubmissionRequirement>
            {
                new SubmissionRequirement 
                { 
                    ReviewRoundID = round.ReviewRoundID, 
                    DocumentName = "Báo cáo tiến độ", 
                    Description = "File PDF/DOCX mô tả tiến độ thực hiện", 
                    AllowedFormats = ".pdf,.docx", 
                    MaxFileSizeMB = 10, 
                    IsRequired = true,
                    Deadline = round.SubmissionDeadline
                },
                new SubmissionRequirement 
                { 
                    ReviewRoundID = round.ReviewRoundID, 
                    DocumentName = "Source code", 
                    Description = "File ZIP chứa mã nguồn project", 
                    AllowedFormats = ".zip,.rar", 
                    MaxFileSizeMB = 50, 
                    IsRequired = true,
                    Deadline = round.SubmissionDeadline
                }
            });
        }

        await _context.SaveChangesAsync();
    }
}
