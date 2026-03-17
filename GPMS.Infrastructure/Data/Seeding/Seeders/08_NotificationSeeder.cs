using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        // Check for specific seeded notifications
        if (await _context.Notifications.AnyAsync(n => n.Title.Contains("Round 2") || n.Title.Contains("nhận xét mới"))) return;

        var round2 = await _context.ReviewRounds.FirstOrDefaultAsync(r => r.RoundNumber == 2 && r.SemesterID == 1);
        var groupMembers = await _context.GroupMembers.Where(gm => gm.Group.GroupName.StartsWith("Team ")).ToListAsync();
        var evaluations = await _context.Evaluations
            .Include(e => e.Group)
            .ThenInclude(pg => pg.GroupMembers)
            .Include(e => e.Feedback)
            .ThenInclude(f => f!.FeedbackApproval)
            .Where(e => e.Feedback != null && e.Feedback.FeedbackApproval != null && e.Feedback.FeedbackApproval.ApprovalStatus == ApprovalStatus.Approved)
            .ToListAsync();

        var notifications = new List<Notification>();

        // 1. Deadline notifications for Round 2
        if (round2 != null)
        {
            foreach (var member in groupMembers)
            {
                notifications.Add(new Notification
                {
                    RecipientID = member.UserID,
                    Title = "Nhắc nhở hạn nộp bài Round 2",
                    Content = $"Hạn nộp bài cho Review Round 2 là ngày {round2.SubmissionDeadline:dd/MM/yyyy}. Vui lòng nộp bài đúng hạn.",
                    Type = NotificationType.Deadline,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                });
            }
        }

        // 2. Feedback notifications for Leaders
        foreach (var eval in evaluations)
        {
            var leader = eval.Group.GroupMembers.FirstOrDefault(gm => gm.RoleInGroup == GroupRole.Leader);
            if (leader != null)
            {
                notifications.Add(new Notification
                {
                    RecipientID = leader.UserID,
                    Title = "Đã có nhận xét mới",
                    Content = $"Phản hồi cho Review Round 1 của nhóm bạn đã được duyệt. Hãy vào xem chi tiết.",
                    Type = NotificationType.Feedback,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                });
            }
        }

        if (notifications.Any())
        {
            _context.Notifications.AddRange(notifications.Take(50)); // Increased limit
            await _context.SaveChangesAsync();
        }
    }
}
