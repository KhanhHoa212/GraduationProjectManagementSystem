using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Data.Seeding.Seeders;

public class LecturerWorkflowTestSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;

    public LecturerWorkflowTestSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public int Order => 9;

    public async Task SeedAsync()
    {
        var today = DateTime.Today;

        await EnsureLecturerCredentialAsync("GV001", "gv001");
        await EnsureLecturerCredentialAsync("GV002", "gv002");
        await EnsureLecturerCredentialAsync("GV003", "gv003");

        var groups = await _context.ProjectGroups.OrderBy(g => g.GroupID).ToListAsync();
        if (groups.Count < 3) return;

        int gId0 = groups[0].GroupID;
        int gId1 = groups[1].GroupID;
        int gId2 = groups[2].GroupID;

        var round1 = await EnsureRoundAsync(
            roundNumber: 1,
            roundType: RoundType.Online,
            startDate: today.AddDays(-14),
            endDate: today.AddDays(7),
            submissionDeadline: today.AddDays(-2),
            status: RoundStatus.Ongoing,
            description: "Round 1 online review workflow test data.");

        var round2 = await EnsureRoundAsync(
            roundNumber: 2,
            roundType: RoundType.Online,
            startDate: today.AddDays(-1),
            endDate: today.AddDays(14),
            submissionDeadline: today.AddDays(3),
            status: RoundStatus.Ongoing,
            description: "Round 2 online review workflow test data.");

        var round3 = await EnsureRoundAsync(
            roundNumber: 3,
            roundType: RoundType.Offline,
            startDate: today.AddDays(10),
            endDate: today.AddDays(21),
            submissionDeadline: today.AddDays(8),
            status: RoundStatus.Planned,
            description: "Round 3 final defense workflow test data.");

        await EnsureChecklistAsync(round1, "Round 1 Checklist", BuildRound1Items());
        await EnsureChecklistAsync(round2, "Round 2 Checklist", BuildRound2Items());
        await EnsureChecklistAsync(round3, "Round 3 Final Defense Rubric", BuildRound3Items());

        var round1Report = await EnsureRequirementAsync(round1, "Round 1 Progress Report", ".pdf,.docx", 15);
        var round2Report = await EnsureRequirementAsync(round2, "Round 2 Interim Report", ".pdf,.docx", 20);
        var round2Code = await EnsureRequirementAsync(round2, "Round 2 Source Package", ".zip,.rar", 100);
        var round3Report = await EnsureRequirementAsync(round3, "Final Report", ".pdf,.docx", 25);
        var round3Slides = await EnsureRequirementAsync(round3, "Defense Slides", ".pdf,.ppt,.pptx", 30);

        await EnsureSubmissionAsync(groupId: gId0, requirementId: round1Report.RequirementID, fileName: "Group100_R1_Report.pdf", fileUrl: "/uploads/test-data/group100/round1-report.pdf", fileSizeMb: 4.2m);
        await EnsureSubmissionAsync(groupId: gId1, requirementId: round1Report.RequirementID, fileName: "Group101_R1_Report.pdf", fileUrl: "/uploads/test-data/group101/round1-report.pdf", fileSizeMb: 4.6m);

        await EnsureSubmissionAsync(groupId: gId0, requirementId: round2Report.RequirementID, fileName: "Group100_R2_Interim_Report.pdf", fileUrl: "/uploads/test-data/group100/round2-interim-report.pdf", fileSizeMb: 5.1m);
        await EnsureSubmissionAsync(groupId: gId0, requirementId: round2Code.RequirementID, fileName: "Group100_R2_Source.zip", fileUrl: "/uploads/test-data/group100/round2-source.zip", fileSizeMb: 24.5m);
        await EnsureSubmissionAsync(groupId: gId1, requirementId: round2Report.RequirementID, fileName: "Group101_R2_Interim_Report.pdf", fileUrl: "/uploads/test-data/group101/round2-interim-report.pdf", fileSizeMb: 4.9m);
        await EnsureSubmissionAsync(groupId: gId1, requirementId: round2Code.RequirementID, fileName: "Group101_R2_Source.zip", fileUrl: "/uploads/test-data/group101/round2-source.zip", fileSizeMb: 22.8m);

        await EnsureSubmissionAsync(groupId: gId2, requirementId: round3Report.RequirementID, fileName: "Group102_R3_Final_Report.pdf", fileUrl: "/uploads/test-data/group102/final-report.pdf", fileSizeMb: 8.3m);
        await EnsureSubmissionAsync(groupId: gId2, requirementId: round3Slides.RequirementID, fileName: "Group102_R3_Defense_Slides.pdf", fileUrl: "/uploads/test-data/group102/defense-slides.pdf", fileSizeMb: 3.4m);

        await EnsureReviewSessionAsync(round2.ReviewRoundID, gId0, today.AddDays(1).AddHours(9), "https://meet.google.com/gpms-r2-ai01", null, "Round 2 interim review for Group 100.");
        await EnsureReviewSessionAsync(round2.ReviewRoundID, gId1, today.AddDays(1).AddHours(14), "https://meet.google.com/gpms-r2-hc01", null, "Round 2 interim review for Group 101.");
        await EnsureReviewSessionAsync(round3.ReviewRoundID, gId2, today.AddDays(12).AddHours(8), null, 3, "Final defense in Hall-A.");

        await EnsureAssignmentAsync(round2.ReviewRoundID, gId0, "GV002");
        await EnsureAssignmentAsync(round2.ReviewRoundID, gId1, "GV001");
        await EnsureAssignmentAsync(round3.ReviewRoundID, gId2, "GV003");

        await EnsureMentorGateAsync(round1.ReviewRoundID, gId0, "GV001", MentorGateStatus.Approved, "Ready for Round 1 external review.");
        await EnsureMentorGateAsync(round1.ReviewRoundID, gId1, "GV002", MentorGateStatus.Approved, "Proceed to Round 1 reviewer evaluation.");
        await EnsureMentorGateAsync(round2.ReviewRoundID, gId0, "GV001", MentorGateStatus.Approved, "Documents are complete. Reviewer can continue.");
        await EnsureMentorGateAsync(round2.ReviewRoundID, gId1, "GV002", MentorGateStatus.Approved, "Interim progress is acceptable for reviewer checking.");
        await EnsureMentorGateAsync(round3.ReviewRoundID, gId2, "GV001", MentorGateStatus.Approved, "Eligible for final defense rehearsal and rubric review.");

        await PatchRound1AssessmentValuesAsync(round1.ReviewRoundID);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureLecturerCredentialAsync(string userId, string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        if (user == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(user.Username))
        {
            user.Username = username;
        }

        var hasInternalCredential = await _context.UserCredentials.AnyAsync(c =>
            c.UserID == userId &&
            c.AuthProvider == AuthProvider.Internal &&
            !string.IsNullOrWhiteSpace(c.PasswordHash));

        if (!hasInternalCredential)
        {
            _context.UserCredentials.Add(new UserCredential
            {
                UserID = userId,
                AuthProvider = AuthProvider.Internal,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
            });
        }
    }

    private async Task<ReviewRound> EnsureRoundAsync(
        int roundNumber,
        RoundType roundType,
        DateTime startDate,
        DateTime endDate,
        DateTime submissionDeadline,
        RoundStatus status,
        string description)
    {
        var round = await _context.ReviewRounds
            .FirstOrDefaultAsync(r => r.SemesterID == 1 && r.RoundNumber == roundNumber);

        if (round == null)
        {
            round = new ReviewRound
            {
                SemesterID = 1,
                RoundNumber = roundNumber
            };

            _context.ReviewRounds.Add(round);
        }

        round.RoundType = roundType;
        round.StartDate = startDate;
        round.EndDate = endDate;
        round.SubmissionDeadline = submissionDeadline;
        round.Status = status;
        round.Description = description;

        await _context.SaveChangesAsync();
        return round;
    }

    private async Task EnsureChecklistAsync(ReviewRound round, string title, IReadOnlyList<ChecklistSeedItem> items)
    {
        var checklist = await _context.ReviewChecklists
            .Include(c => c.ChecklistItems)
            .FirstOrDefaultAsync(c => c.ReviewRoundID == round.ReviewRoundID);

        if (checklist == null)
        {
            checklist = new ReviewChecklist
            {
                ReviewRoundID = round.ReviewRoundID,
                Title = title,
                Description = title,
                CreatedBy = "GV001",
                CreatedAt = DateTime.UtcNow
            };

            _context.ReviewChecklists.Add(checklist);
            await _context.SaveChangesAsync();

            checklist = await _context.ReviewChecklists
                .Include(c => c.ChecklistItems)
                .FirstAsync(c => c.ReviewRoundID == round.ReviewRoundID);
        }
        else
        {
            checklist.Title = title;
            checklist.Description = title;
            checklist.CreatedBy = checklist.CreatedBy ?? "GV001";
        }

        foreach (var seedItem in items)
        {
            var item = checklist.ChecklistItems.FirstOrDefault(i => i.ItemCode == seedItem.ItemCode);
            if (item == null)
            {
                item = new ChecklistItem
                {
                    ChecklistID = checklist.ChecklistID,
                    ItemCode = seedItem.ItemCode
                };

                _context.ChecklistItems.Add(item);
                checklist.ChecklistItems.Add(item);
            }

            item.ItemName = seedItem.ItemName;
            item.ItemContent = seedItem.ItemContent;
            item.Section = seedItem.SectionTitle;
            item.ItemType = seedItem.InputType.ToString();
            item.OrderIndex = seedItem.OrderIndex;

            /*
            // Clear and re-add rubrics
            var existingRubrics = await _context.RubricDescriptions.Where(r => r.ItemID == item.ItemID).ToListAsync();
            _context.RubricDescriptions.RemoveRange(existingRubrics);
            */

            if (!string.IsNullOrWhiteSpace(seedItem.ExcellentRubric))
                _context.RubricDescriptions.Add(new RubricDescription { Item = item, GradeLevel = "Excellent", Description = seedItem.ExcellentRubric });
            if (!string.IsNullOrWhiteSpace(seedItem.GoodRubric))
                _context.RubricDescriptions.Add(new RubricDescription { Item = item, GradeLevel = "Good", Description = seedItem.GoodRubric });
            if (!string.IsNullOrWhiteSpace(seedItem.AcceptableRubric))
                _context.RubricDescriptions.Add(new RubricDescription { Item = item, GradeLevel = "Acceptable", Description = seedItem.AcceptableRubric });
            if (!string.IsNullOrWhiteSpace(seedItem.FailRubric))
                _context.RubricDescriptions.Add(new RubricDescription { Item = item, GradeLevel = "Fail", Description = seedItem.FailRubric });
        }

        await _context.SaveChangesAsync();
    }

    private async Task<SubmissionRequirement> EnsureRequirementAsync(ReviewRound round, string documentName, string formats, int maxFileSizeMb)
    {
        var requirement = await _context.SubmissionRequirements
            .FirstOrDefaultAsync(r => r.ReviewRoundID == round.ReviewRoundID && r.DocumentName == documentName);

        if (requirement == null)
        {
            requirement = new SubmissionRequirement
            {
                ReviewRoundID = round.ReviewRoundID,
                DocumentName = documentName
            };

            _context.SubmissionRequirements.Add(requirement);
        }

        requirement.Description = $"{documentName} for Round {round.RoundNumber}.";
        requirement.AllowedFormats = formats;
        requirement.MaxFileSizeMB = maxFileSizeMb;
        requirement.IsRequired = true;
        requirement.Deadline = round.SubmissionDeadline;

        await _context.SaveChangesAsync();
        return requirement;
    }

    private async Task EnsureSubmissionAsync(int groupId, int requirementId, string fileName, string fileUrl, decimal fileSizeMb)
    {
        var submission = await _context.Submissions
            .Include(s => s.Requirement)
            .Where(s => s.GroupID == groupId && s.RequirementID == requirementId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync();

        var leaderId = await _context.GroupMembers
            .Where(m => m.GroupID == groupId && m.RoleInGroup == GroupRole.Leader)
            .Select(m => m.UserID)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(leaderId))
        {
            return;
        }

        var deadline = await _context.SubmissionRequirements
            .Where(r => r.RequirementID == requirementId)
            .Select(r => r.Deadline)
            .FirstAsync();

        if (submission == null)
        {
            submission = new Submission
            {
                GroupID = groupId,
                RequirementID = requirementId,
                Version = 1
            };

            _context.Submissions.Add(submission);
        }

        submission.FileName = fileName;
        submission.FileUrl = fileUrl;
        submission.FileSizeMB = fileSizeMb;
        submission.SubmittedBy = leaderId;
        submission.SubmittedAt = deadline.AddHours(-6);
        submission.Status = SubmissionStatus.OnTime;
    }

    private async Task EnsureReviewSessionAsync(int roundId, int groupId, DateTime scheduledAt, string? meetLink, int? roomId, string notes)
    {
        var session = await _context.ReviewSessions
            .FirstOrDefaultAsync(s => s.ReviewRoundID == roundId && s.GroupID == groupId);

        if (session == null)
        {
            session = new ReviewSessionInfo
            {
                ReviewRoundID = roundId,
                GroupID = groupId
            };

            _context.ReviewSessions.Add(session);
        }

        session.ScheduledAt = scheduledAt;
        session.MeetLink = meetLink;
        session.RoomID = roomId;
        session.Notes = notes;
    }

    private async Task EnsureAssignmentAsync(int roundId, int groupId, string reviewerId)
    {
        var assignment = await _context.ReviewerAssignments
            .FirstOrDefaultAsync(a => a.ReviewRoundID == roundId && a.GroupID == groupId && a.ReviewerID == reviewerId);

        if (assignment == null)
        {
            assignment = new ReviewerAssignment
            {
                ReviewRoundID = roundId,
                GroupID = groupId,
                ReviewerID = reviewerId
            };

            _context.ReviewerAssignments.Add(assignment);
        }

        assignment.AssignedAt = DateTime.UtcNow.AddDays(-1);
        assignment.AssignedBy = "ADMIN001";
        assignment.IsRandom = false;
    }

    private async Task EnsureMentorGateAsync(int roundId, int groupId, string supervisorId, MentorGateStatus status, string comment)
    {
        var gate = await _context.MentorRoundReviews
            .FirstOrDefaultAsync(g => g.ReviewRoundID == roundId && g.GroupID == groupId);

        if (gate == null)
        {
            gate = new MentorRoundReview
            {
                ReviewRoundID = roundId,
                GroupID = groupId,
                SupervisorID = supervisorId
            };

            _context.MentorRoundReviews.Add(gate);
        }

        gate.SupervisorID = supervisorId;
        gate.DecisionStatus = status;
        gate.ProgressComment = comment;
        gate.ReviewedAt = DateTime.UtcNow.AddHours(-12);
        gate.ReviewerNotifiedAt = status == MentorGateStatus.Approved ? DateTime.UtcNow.AddHours(-11) : null;
    }

    private async Task PatchRound1AssessmentValuesAsync(int round1Id)
    {
        var details = await _context.EvaluationDetails
            .Include(d => d.Evaluation)
            .Include(d => d.Item)
            .Where(d => d.Evaluation.ReviewRoundID == round1Id)
            .ToListAsync();

        foreach (var detail in details)
        {
            detail.Assessment ??= "Y";
        }
    }

    private static List<ChecklistSeedItem> BuildRound1Items() =>
        new()
        {
            new("R1-P1-01", "Coverage of Objectives", "Objectives and current scope are clearly covered in the submission.", "P1", "Coverage of Objectives", "Mandatory", ChecklistInputType.YesNoNa, 1, 1m),
            new("R1-P1-02", "Coverage of Objectives", "Project features align with the approved problem statement.", "P1", "Coverage of Objectives", "Mandatory", ChecklistInputType.YesNoNa, 2, 1m),
            new("R1-D1-01", "URD-SRS Overview", "URD and SRS overview is understandable for reviewer and mentor.", "D1", "URD-SRS Overview", "Mandatory", ChecklistInputType.YesNoNa, 3, 1m),
            new("R1-D1-02", "Correctness", "Main functional requirements are internally consistent.", "D1", "Correctness", "Mandatory", ChecklistInputType.YesNoNa, 4, 1m),
            new("R1-QA-01", "Quality Attributes", "Relevant quality attributes are identified for this stage.", "D1", "Quality Attributes", null, ChecklistInputType.YesNoNa, 5, 1m),
            new("R1-AI-01", "Apply AI in Project", "AI usage is explained with a realistic project need.", "AI", "Apply AI in Project", null, ChecklistInputType.YesNoNa, 6, 1m)
        };

    private static List<ChecklistSeedItem> BuildRound2Items() =>
        new()
        {
            new("R2-A-01", "Software Product", "Core software product features are implemented and demo-ready.", "A", "Software Product", null, ChecklistInputType.YesNoNa, 1, 1m),
            new("R2-A-02", "Software Product", "Interim report reflects the current product state accurately.", "A", "Software Product", null, ChecklistInputType.YesNoNa, 2, 1m),
            new("R2-B-01", "Project Management", "The team follows a reasonable project plan and task breakdown.", "B", "Project Management", null, ChecklistInputType.YesNoNa, 3, 1m),
            new("R2-C-01", "Interaction with Supervisor", "The group responds to supervisor feedback consistently.", "C", "Interaction with Supervisor(s)", null, ChecklistInputType.YesNoNa, 4, 1m),
            new("R2-D-01", "Interaction with Third Parties", "External dependencies and third-party integration are managed clearly.", "D", "Interaction with Third Parties", null, ChecklistInputType.YesNoNa, 5, 1m),
            new("R2-AI-01", "Apply AI", "AI progress is integrated into the software increment, not only described.", "AI", "Apply AI", null, ChecklistInputType.YesNoNa, 6, 1m)
        };

    private static List<ChecklistSeedItem> BuildRound3Items() =>
        new()
        {
            new("R3-P1", "Scope", "Coverage of Objectives", "P", "Product", null, ChecklistInputType.GradeLevel, 1, 1m,
                "Covers all approved objectives with clear evidence.", "Covers most approved objectives with minor gaps.", "Covers only the core objectives with notable gaps.", "Fails to cover the approved objectives."),
            new("R3-P4", "UI/UX", "User interface is coherent, usable, and aligned with user flow.", "P", "Product", null, ChecklistInputType.GradeLevel, 2, 1m,
                "UI is polished and highly usable.", "UI is consistent with small usability issues.", "UI is usable but inconsistent in several places.", "UI is confusing or incomplete."),
            new("R3-D1", "Requirements", "User Requirement and System Requirement document quality.", "D", "Document", null, ChecklistInputType.GradeLevel, 3, 1m,
                "Requirements are complete, testable, and well-structured.", "Requirements are mostly clear with minor omissions.", "Requirements are partially complete and need clarification.", "Requirements are incomplete or unclear."),
            new("R3-PE1", "Presentation Skills", "Presentation delivery, structure, and confidence.", "PE", "Presentation", null, ChecklistInputType.GradeLevel, 4, 1m,
                "Presentation is clear, confident, and persuasive.", "Presentation is clear with minor delivery issues.", "Presentation is understandable but lacks confidence.", "Presentation is weak or hard to follow."),
            new("R3-AI1", "AI Problem Selection", "Appropriateness of AI problem selection for the project.", "AI", "AI", null, ChecklistInputType.GradeLevel, 5, 1m,
                "AI problem selection is highly suitable and well-justified.", "AI problem selection is suitable with adequate justification.", "AI problem selection is partly suitable but weakly justified.", "AI problem selection is not suitable for the project.")
        };
}

internal sealed record ChecklistSeedItem(
    string ItemCode,
    string ItemName,
    string ItemContent,
    string SectionCode,
    string SectionTitle,
    string? PriorityLabel,
    ChecklistInputType InputType,
    int OrderIndex,
    decimal Weight,
    string? ExcellentRubric = null,
    string? GoodRubric = null,
    string? AcceptableRubric = null,
    string? FailRubric = null,
    decimal MaxScore = 10m);
