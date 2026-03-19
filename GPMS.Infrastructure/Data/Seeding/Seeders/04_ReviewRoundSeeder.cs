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
                CreatedAt = DateTime.UtcNow,
                Type = round.RoundNumber == 3 ? ChecklistType.Rubric : ChecklistType.YesNo
            };
            _context.ReviewChecklists.Add(checklist);
            await _context.SaveChangesAsync();

            // Items
            var items = new List<ChecklistItem>();

            if (round.RoundNumber < 3)
            {
                items.AddRange(new List<ChecklistItem>
                {
                    new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = $"R{round.RoundNumber}Q1", ItemContent = "Sinh viên chuẩn bị đầy đủ tài liệu?", ItemType = "YesNo", Section = "Preparation", OrderIndex = 1 },
                    new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = $"R{round.RoundNumber}Q2", ItemContent = "Mã nguồn được upload lên repo đầy đủ?", ItemType = "YesNo", Section = "Technical", OrderIndex = 2 },
                    new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = $"R{round.RoundNumber}Q3", ItemContent = "Sinh viên nắm vững kiến thức đã thực hiện?", ItemType = "YesNo", Section = "Knowledge", OrderIndex = 3 }
                });
            }
            else // Round 3 - Rubric
            {
                var rubricItem = new ChecklistItem 
                { 
                    ChecklistID = checklist.ChecklistID, 
                    ItemCode = "R3C1", 
                    ItemName = "Product Quality", 
                    ItemContent = "Đánh giá chất lượng sản phẩm phần mềm", 
                    ItemType = "Rubric", 
                    Section = "Software Product", 
                    OrderIndex = 1 
                };
                items.Add(rubricItem);
                
                // We'll add RubricDescriptions after saving items to get IDs if needed, 
                // but since we are using EF, we can add to the collection.
                rubricItem.RubricDescriptions.Add(new RubricDescription { GradeLevel = "Excellent", Description = "Sản phẩm hoàn thiện, không lỗi, UX tốt." });
                rubricItem.RubricDescriptions.Add(new RubricDescription { GradeLevel = "Good", Description = "Sản phẩm chạy tốt, còn vài lỗi nhỏ không đáng kể." });
                rubricItem.RubricDescriptions.Add(new RubricDescription { GradeLevel = "Acceptable", Description = "Sản phẩm đáp ứng yêu cầu tối thiểu." });
                rubricItem.RubricDescriptions.Add(new RubricDescription { GradeLevel = "Fail", Description = "Sản phẩm chưa hoàn thiện hoặc có lỗi nghiêm trọng." });
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
