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
        var faker = new Faker("vi");

        var assignments = await _context.ReviewerAssignments
            .Include(ra => ra.ReviewRound)
                .ThenInclude(rr => rr!.ReviewChecklist)
                    .ThenInclude(rc => rc!.ChecklistItems)
            .Include(ra => ra.Group)
            .Where(ra => ra.ReviewRound.Status == RoundStatus.Completed || ra.ReviewRound.Status == RoundStatus.Ongoing)
            .ToListAsync();

        if (!assignments.Any()) return;

        var evaluations = new List<Evaluation>();
        var details = new List<EvaluationDetail>();
        var feedbacks = new List<Feedback>();
        var approvals = new List<FeedbackApproval>();

        var supervisors = await _context.ProjectSupervisors.ToListAsync();

        var existingEvalKeys = new HashSet<(int, int, string)>(
            await _context.Evaluations.Select(e => new ValueTuple<int, int, string>(e.GroupID, e.ReviewRoundID, e.ReviewerID)).ToListAsync()
        );

        int count = 0;
        foreach (var ra in assignments)
        {
            var checklist = ra.ReviewRound.ReviewChecklist;
            if (checklist == null) continue;
            
            // Randomly skipping some evaluations to mock incomplete data
            if (ra.ReviewRound.Status == RoundStatus.Ongoing && faker.Random.Double() > 0.7) continue;

            // Check if evaluation already exists - Using HashSet
            if (existingEvalKeys.Contains((ra.GroupID, ra.ReviewRoundID, ra.ReviewerID))) 
                continue;

            var evaluation = new Evaluation
            {
                ReviewRoundID = ra.ReviewRoundID,
                GroupID = ra.GroupID,
                ReviewerID = ra.ReviewerID,
                Status = EvaluationStatus.Submitted,
                SubmittedAt = ra.AssignedAt.AddDays(faker.Random.Int(2, 7)),
                OverallComment = faker.Lorem.Paragraphs(1)
            };

            evaluations.Add(evaluation);

            foreach (var item in checklist.ChecklistItems)
            {
                string assessment = "Yes"; 
                if (item.ItemType == "Rubric")
                {
                    var grades = new[] { "Excellent", "Good", "Acceptable", "Fail" };
                    assessment = faker.PickRandom(new[] { "Excellent", "Good", "Good", "Acceptable", "Acceptable", "Acceptable", "Fail" });
                }
                else
                {
                    var answers = new[] { "Yes", "Yes", "No", "N/A" };
                    assessment = faker.PickRandom(answers);
                }
                
                details.Add(new EvaluationDetail
                {
                    Evaluation = evaluation,
                    ItemID = item.ItemID,
                    Assessment = assessment,
                    Comment = faker.Lorem.Sentence()
                });
            }

            var feedback = new Feedback
            {
                Evaluation = evaluation,
                Content = faker.Lorem.Sentences(2),
                CreatedAt = (evaluation.SubmittedAt ?? DateTime.UtcNow).AddHours(faker.Random.Int(1, 24))
            };
            feedbacks.Add(feedback);

            var isApproved = count % 10 != 0; // 90% approved
            if (isApproved)
            {
                var supervisor = supervisors.FirstOrDefault(ps => ps.ProjectID == ra.Group?.ProjectID && ps.Role == ProjectRole.Main);
                if (supervisor != null)
                {
                    approvals.Add(new FeedbackApproval
                    {
                        Feedback = feedback,
                        SupervisorID = supervisor.LecturerID,
                        ApprovalStatus = ApprovalStatus.Approved,
                        ApprovedAt = feedback.CreatedAt.AddHours(faker.Random.Int(1, 24)),
                        SupervisorComment = "Nhận xét chân thực."
                    });
                }
            }
            else
            {
                approvals.Add(new FeedbackApproval
                {
                    Feedback = feedback,
                    ApprovalStatus = ApprovalStatus.Pending
                });
            }

            count++;
        }

        try 
        {
            if (evaluations.Any()) await _context.Evaluations.AddRangeAsync(evaluations);
            if (details.Any()) await _context.EvaluationDetails.AddRangeAsync(details);
            if (feedbacks.Any()) await _context.Feedbacks.AddRangeAsync(feedbacks);
            if (approvals.Any()) await _context.FeedbackApprovals.AddRangeAsync(approvals);
            
            if (evaluations.Any() || details.Any() || feedbacks.Any() || approvals.Any())
                await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            Console.WriteLine($"[Seeding] EvaluationSeeder FAILED: {msg}");
        }
    }
}

