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

public class NotificationSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public int Order => 8;

    public NotificationSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Notifications.AnyAsync()) return;

        var faker = new Faker("vi");

        var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.Status == SemesterStatus.Active);
        if (activeSemester == null) return;

        var rounds = await _context.ReviewRounds
            .Where(r => r.SemesterID == activeSemester.SemesterID)
            .ToListAsync();
            
        var groupMembers = await _context.GroupMembers
            .Include(gm => gm.Group)
                .ThenInclude(g => g.Project)
            .Where(gm => gm.Group.Project.SemesterID == activeSemester.SemesterID)
            .ToListAsync();

        var evaluations = await _context.Evaluations
            .Include(e => e.Group)
            .ThenInclude(pg => pg.GroupMembers)
            .Where(e => e.ReviewRound.SemesterID == activeSemester.SemesterID)
            .ToListAsync();

        var notifications = new List<Notification>();

        // 1. Deadline notifications
        var round2 = rounds.FirstOrDefault(r => r.RoundNumber == 2);
        if (round2 != null)
        {
            var targetMembers = groupMembers.Take(100).ToList();
            foreach (var member in targetMembers)
            {
                notifications.Add(new Notification
                {
                    RecipientID = member.UserID,
                    Title = "Nhắc nhở hạn nộp bài Round 2",
                    Content = $"Hạn nộp bài cho Review Round 2 là ngày {round2.SubmissionDeadline:dd/MM/yyyy}. Vui lòng nộp bài đúng hạn.",
                    Type = NotificationType.Deadline,
                    IsRead = faker.Random.Bool(),
                    CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 3))
                });
            }
        }

        // 2. Feedback notifications
        foreach (var eval in evaluations.Take(50))
        {
            var leader = eval.Group.GroupMembers.FirstOrDefault(gm => gm.RoleInGroup == GroupRole.Leader);
            if (leader != null)
            {
                notifications.Add(new Notification
                {
                    RecipientID = leader.UserID,
                    Title = "Đã có nhận xét mới",
                    Content = $"Phản hồi cho Review Round {eval.ReviewRoundID % 3 + 1} của nhóm bạn đã được cập nhật.",
                    Type = NotificationType.Feedback,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-faker.Random.Int(1, 12))
                });
            }
        }

        if (notifications.Any())
        {
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }
    }
}

