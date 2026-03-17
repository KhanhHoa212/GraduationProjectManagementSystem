using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Data.Seeding.Seeders;

public class EvaluationSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public int Order => 7;

    public EvaluationSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check for specific seeded evaluations
        if (await _context.Evaluations.AnyAsync(e => e.ReviewRound.SemesterID == 1)) return;

        var assignments = await _context.ReviewerAssignments
            .Where(ra => ra.ReviewRound.RoundNumber == 1 && ra.ReviewRound.SemesterID == 1)
            .Include(ra => ra.ReviewRound)
            .ThenInclude(rr => rr!.ReviewChecklist)
            .ThenInclude(rc => rc!.ChecklistItems)
            .ToListAsync();

        var random = new Random();
        int count = 0;

        foreach (var ra in assignments)
        {
            var checklist = ra.ReviewRound.ReviewChecklist;
            if (checklist == null) continue;

            var evaluation = new Evaluation
            {
                ReviewRoundID = ra.ReviewRoundID,
                GroupID = ra.GroupID,
                ReviewerID = ra.ReviewerID,
                Status = EvaluationStatus.Submitted,
                SubmittedAt = DateTime.UtcNow.AddDays(-1),
                TotalScore = 0 // Will update after adding details
            };

            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();

            decimal totalScore = 0;
            foreach (var item in checklist.ChecklistItems!)
            {
                var maxScoreInt = (int)item.MaxScore;
                var scoreValue = random.Next((int)(maxScoreInt * 0.7), maxScoreInt + 1);
                var score = (decimal)scoreValue;
                
                totalScore += score;
                _context.EvaluationDetails.Add(new EvaluationDetail
                {
                    EvaluationID = evaluation.EvaluationID,
                    ItemID = item.ItemID,
                    Score = score,
                    Comment = $"Đạt yêu cầu: {item.ItemContent}"
                });
            }

            evaluation.TotalScore = totalScore;

            // Feedback
            var feedback = new Feedback
            {
                EvaluationID = evaluation.EvaluationID,
                Content = $"Bài làm của nhóm khá tốt. Cần phát huy ở các round tiếp theo.",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            };
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            // Feedback Approval: 8 Approved, 2 Pending
            if (count < 8)
            {
                var supervisor = await _context.ProjectSupervisors
                    .FirstOrDefaultAsync(ps => ps.Project.ProjectGroups.Any(pg => pg.GroupID == ra.GroupID) && ps.Role == ProjectRole.Main);

                if (supervisor != null)
                {
                    _context.FeedbackApprovals.Add(new FeedbackApproval
                    {
                        FeedbackID = feedback.FeedbackID,
                        SupervisorID = supervisor.LecturerID,
                        ApprovalStatus = ApprovalStatus.Approved,
                        ApprovedAt = DateTime.UtcNow.AddHours(-2),
                        SupervisorComment = "Đồng ý với nhận xét của reviewer."
                    });
                }
            }
            else
            {
                _context.FeedbackApprovals.Add(new FeedbackApproval
                {
                    FeedbackID = feedback.FeedbackID,
                    ApprovalStatus = ApprovalStatus.Pending
                });
            }

            count++;
        }

        await _context.SaveChangesAsync();
    }
}
