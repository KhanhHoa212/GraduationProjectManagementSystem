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
        var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.Status == SemesterStatus.Active);
        if (activeSemester == null) return;

        int semesterId = activeSemester.SemesterID;
        int year = activeSemester.StartDate.Year;

        var rounds = await _context.ReviewRounds.Where(r => r.SemesterID == semesterId).ToListAsync();
        if (!rounds.Any())
        {
            rounds = new List<ReviewRound>
            {
                new ReviewRound
                {
                    SemesterID = semesterId,
                    RoundNumber = 1,
                    RoundType = RoundType.Online,
                    Status = RoundStatus.Completed,
                    StartDate = new DateTime(year, 1, 15),
                    EndDate = new DateTime(year, 2, 15),
                    SubmissionDeadline = new DateTime(year, 2, 10)
                },
                new ReviewRound
                {
                    SemesterID = semesterId,
                    RoundNumber = 2,
                    RoundType = RoundType.Offline,
                    Status = RoundStatus.Ongoing,
                    StartDate = new DateTime(year, 2, 20),
                    EndDate = new DateTime(year, 3, 20),
                    SubmissionDeadline = new DateTime(year, 3, 15)
                },
                new ReviewRound
                {
                    SemesterID = semesterId,
                    RoundNumber = 3,
                    RoundType = RoundType.Offline,
                    Status = RoundStatus.Planned,
                    StartDate = new DateTime(year, 4, 1),
                    EndDate = new DateTime(year, 4, 20),
                    SubmissionDeadline = new DateTime(year, 4, 15)
                }
            };

            _context.ReviewRounds.AddRange(rounds);
            await _context.SaveChangesAsync();
        }

        foreach (var round in rounds)
        {
            // Checklist
            var checklist = await _context.ReviewChecklists
                .Include(c => c.ChecklistItems)
                    .ThenInclude(i => i.EvaluationDetails)
                .Include(c => c.ChecklistItems)
                    .ThenInclude(i => i.RubricDescriptions)
                .FirstOrDefaultAsync(c => c.ReviewRoundID == round.ReviewRoundID);

            if (checklist == null)
            {
                // ... (existing checklist creation logic)
                checklist = new ReviewChecklist
                {
                    ReviewRoundID = round.ReviewRoundID,
                    Title = $"Checklist for Round {round.RoundNumber}",
                    Description = $"Detailed evaluation criteria for Review Round {round.RoundNumber}",
                    CreatedBy = "HOD001",
                    CreatedAt = DateTime.UtcNow,
                    Type = round.RoundNumber == 3 ? ChecklistType.Rubric : ChecklistType.YesNo
                };
                _context.ReviewChecklists.Add(checklist);
                await _context.SaveChangesAsync();
            }

            // Items - Re-seed if no items or if they are the old placeholders
            bool hasNewItems = checklist.ChecklistItems.Any(i => i.ItemCode == "P1-1" || i.ItemCode == "A-1" || i.ItemCode == "P1");

            if (!hasNewItems)
            {
                if (checklist.ChecklistItems.Any())
                {
                    // Clean up dependent evaluations and feedbacks first to avoid FK constraint issues
                    var itemIds = checklist.ChecklistItems.Select(i => i.ItemID).ToList();
                    var dependentDetails = await _context.EvaluationDetails.Where(d => itemIds.Contains(d.ItemID)).ToListAsync();
                    
                    if (dependentDetails.Any())
                    {
                        var evaluationIds = dependentDetails.Select(d => d.EvaluationID).Distinct().ToList();
                        
                        // Delete Feedbacks and Approvals
                        var feedbacks = await _context.Feedbacks.Where(f => evaluationIds.Contains(f.EvaluationID)).ToListAsync();
                        if (feedbacks.Any())
                        {
                            var feedbackIds = feedbacks.Select(f => f.FeedbackID).ToList();
                            var approvals = await _context.FeedbackApprovals.Where(a => feedbackIds.Contains(a.FeedbackID)).ToListAsync();
                            _context.FeedbackApprovals.RemoveRange(approvals);
                            _context.Feedbacks.RemoveRange(feedbacks);
                        }

                        _context.EvaluationDetails.RemoveRange(dependentDetails);
                        
                        // Delete Evaluations
                        var evaluations = await _context.Evaluations.Where(e => evaluationIds.Contains(e.EvaluationID)).ToListAsync();
                        _context.Evaluations.RemoveRange(evaluations);

                        await _context.SaveChangesAsync();
                    }

                    _context.ChecklistItems.RemoveRange(checklist.ChecklistItems);
                    await _context.SaveChangesAsync();
                }

                var items = new List<ChecklistItem>();

                if (round.RoundNumber == 1)
                {
                    items.AddRange(new List<ChecklistItem>
                    {
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P1-1", ItemContent = "Is problem to solve stated clearly?", ItemType = "YesNo", Section = "P1- Coverage of Objectives", OrderIndex = 1, ItemName = "Mandatory" },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P1-2", ItemContent = "Is a functional overview of the system provided?", ItemType = "YesNo", Section = "P1- Coverage of Objectives", OrderIndex = 2 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P1-3", ItemContent = "If assumptions that affect implementation have been made, are they stated?", ItemType = "YesNo", Section = "P1- Coverage of Objectives", OrderIndex = 3 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P1-4", ItemContent = "Is size of system (Usecase Points) larged enough for members to do in", ItemType = "YesNo", Section = "P1- Coverage of Objectives", OrderIndex = 4, ItemName = "Mandatory" },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P2-1", ItemContent = "Does project have real users with real problems (pain/gain points)?", ItemType = "YesNo", Section = "P2-Practical applicability", OrderIndex = 5 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P2-2", ItemContent = "Does project team work with real stakeholders?", ItemType = "YesNo", Section = "P2-Practical applicability", OrderIndex = 6 },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P3-1", ItemContent = "Does project introduce new business model or localize successful global", ItemType = "YesNo", Section = "P3-Innovation and Creativity", OrderIndex = 7 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P3-2", ItemContent = "Does project apply new technologies, algorithms or UI/UX (AR,VR,voice...)", ItemType = "YesNo", Section = "P3-Innovation and Creativity", OrderIndex = 8 },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-1", ItemContent = "Do the requirements provide an adequate basis for design?", ItemType = "YesNo", Section = "D1-URD-SRS-Overview", OrderIndex = 9, ItemName = "Mandatory" },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-2", ItemContent = "Is the implementation priority of each requirement included?", ItemType = "YesNo", Section = "D1-URD-SRS-Overview", OrderIndex = 10, ItemName = "Mandatory" },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-3", ItemContent = "Are algorithms intrinsic to the use-case defined?", ItemType = "YesNo", Section = "D1-URD-SRS-Overview", OrderIndex = 11 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-4", ItemContent = "Is any necessary information missing from a requirement? If so, it is identified as to-be-determined marker", ItemType = "YesNo", Section = "D1-URD-SRS-Overview", OrderIndex = 12 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-5", ItemContent = "Does the team use workflow tools to model the system's business flow?", ItemType = "YesNo", Section = "D1-URD-SRS-Overview", OrderIndex = 13, ItemName = "Mandatory" },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-6", ItemContent = "Do any requirements conflict with or duplicate other requirements?", ItemType = "YesNo", Section = "D1- Correctness", OrderIndex = 14 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-7", ItemContent = "Is each requirement verifiable (such as by review, testing, demonstration, or analysis)?", ItemType = "YesNo", Section = "D1- Correctness", OrderIndex = 15, ItemName = "Mandatory" },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-8", ItemContent = "Is each requirement in scope for the project?", ItemType = "YesNo", Section = "D1- Correctness", OrderIndex = 16 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-9", ItemContent = "Can all of the requirements be implemented within known constraints?", ItemType = "YesNo", Section = "D1- Correctness", OrderIndex = 17, ItemName = "Mandatory" },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-10", ItemContent = "Are all performance objectives properly specified?", ItemType = "YesNo", Section = "D1- Quality attributes", OrderIndex = 18, ItemName = "Mandatory" },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-11", ItemContent = "Are all security and safety considerations properly specified?", ItemType = "YesNo", Section = "D1- Quality attributes", OrderIndex = 19 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-12", ItemContent = "Are other pertinent quality attribute goals explicitly documented and quantified, with the acceptable trade-offs specified?", ItemType = "YesNo", Section = "D1- Quality attributes", OrderIndex = 20 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-13", ItemContent = "Check whether interfaces with other systems are described", ItemType = "YesNo", Section = "D1- Quality attributes", OrderIndex = 21, ItemName = "Mandatory" },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-14", ItemContent = "Check whether the required hardware is described", ItemType = "YesNo", Section = "D1- Quality attributes", OrderIndex = 22 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-15", ItemContent = "Check whether specific software requirements are documented", ItemType = "YesNo", Section = "D1- Quality attributes", OrderIndex = 23 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-16", ItemContent = "Check whether networking Issues and connectivity requirements are", ItemType = "YesNo", Section = "D1- Quality attributes", OrderIndex = 24 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-17", ItemContent = "Check whether specific communication requirements are documented", ItemType = "YesNo", Section = "D1- Quality attributes", OrderIndex = 25 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-18", ItemContent = "Check whether availability requirements are documented", ItemType = "YesNo", Section = "D1- Quality attributes", OrderIndex = 26 },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-19", ItemContent = "Multi Language support", ItemType = "YesNo", Section = "D1- Special user requirements", OrderIndex = 27 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1-20", ItemContent = "Is any security required?", ItemType = "YesNo", Section = "D1- Special user requirements", OrderIndex = 28 },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI-1", ItemContent = "Are the reasons for using AI and the specific problem that AI solves in the topic clearly stated?", ItemType = "YesNo", Section = "AI- Apply AI inproject", OrderIndex = 29 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI-2", ItemContent = "Does the project develop an AI model or an equivalent product component such as an AI Agent or just use an AI service (such as OpenAPI, Google Gemini API).", ItemType = "YesNo", Section = "AI- Apply AI inproject", OrderIndex = 30 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI-3", ItemContent = "Is the workflow for processing AI features in the system described briefly, clearly and reasonably?", ItemType = "YesNo", Section = "AI- Apply AI inproject", OrderIndex = 31 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI-4", ItemContent = "Are the functional and non-functional requirements related to AI fully and clearly described in the requirements specification document?", ItemType = "YesNo", Section = "AI- Apply AI inproject", OrderIndex = 32 },
                    });
                }
                else if (round.RoundNumber == 2)
                {
                    items.AddRange(new List<ChecklistItem>
                    {
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "A-1", ItemContent = "Does the system architecture, including sub-systems and their interconnections, clearly documented in both deployment and process views?", ItemType = "YesNo", Section = "A. Software Product", OrderIndex = 1 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "A-2", ItemContent = "Does the Entity-Relationship Diagram (ERD) accurately represent all entities, their relationships, and attributes within the system?", ItemType = "YesNo", Section = "A. Software Product", OrderIndex = 2 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "A-3", ItemContent = "Are the main entity models comprehensive enough to encapsulate all required states and behaviors as illustrated in the state diagrams of key system entities?", ItemType = "YesNo", Section = "A. Software Product", OrderIndex = 3 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "A-4", ItemContent = "Is the use of external services (such as third-party APIs) justified and implemented effectively within the project?", ItemType = "YesNo", Section = "A. Software Product", OrderIndex = 4 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "A-5", ItemContent = "Is the source code structure organized in a manner that supports logical understanding, maintainability, and potential for future scalability?", ItemType = "YesNo", Section = "A. Software Product", OrderIndex = 5 },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "B-1", ItemContent = "Are tasks fairly and appropriately distributed among members considering their interests and skills?", ItemType = "YesNo", Section = "B. Project Management", OrderIndex = 6 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "B-2", ItemContent = "Is there effective communication and collaboration among team members?", ItemType = "YesNo", Section = "B. Project Management", OrderIndex = 7 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "B-3", ItemContent = "Does the team meet deadlines consistently?", ItemType = "YesNo", Section = "B. Project Management", OrderIndex = 8 },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "C-1", ItemContent = "Are regular meetings with the Supervisor organized?", ItemType = "YesNo", Section = "C. Interaction with Supervisor(s)", OrderIndex = 9 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "C-2", ItemContent = "Does the team proactively discuss problems and seek guidance from the supervisor?", ItemType = "YesNo", Section = "C. Interaction with Supervisor(s)", OrderIndex = 10 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "C-3", ItemContent = "Does the team actively receive and process feedback from the Supervisor?", ItemType = "YesNo", Section = "C. Interaction with Supervisor(s)", OrderIndex = 11 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "C-4", ItemContent = "Are updates on progress and results regularly communicated to the Supervisor?", ItemType = "YesNo", Section = "C. Interaction with Supervisor(s)", OrderIndex = 12 },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D-1", ItemContent = "Is there effective communication with third parties such as Reviewers or businesses?", ItemType = "YesNo", Section = "D. Interaction with Third Parties", OrderIndex = 13 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D-2", ItemContent = "Does the team handle requests or feedback from third parties professionally and promptly?", ItemType = "YesNo", Section = "D. Interaction with Third Parties", OrderIndex = 14 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D-3", ItemContent = "Are contributions from third parties integrated into the project?", ItemType = "YesNo", Section = "D. Interaction with Third Parties", OrderIndex = 15 },

                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI-1", ItemContent = "System Architecture or package diagram describing AI as a separate or embedded component of the main processing logic?", ItemType = "YesNo", Section = "AI. Apply AI", OrderIndex = 16 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI-2", ItemContent = "Can sequence diagrams show how the system sends and receives data with AI?", ItemType = "YesNo", Section = "AI. Apply AI", OrderIndex = 17 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI-3", ItemContent = "Is there a sequence diagram or detailed description clearly stating where the system calls the AI and how it waits for a response?", ItemType = "YesNo", Section = "AI. Apply AI", OrderIndex = 18 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI-4", ItemContent = "Does the database design support saving data information sent to AI and the AI results returned?", ItemType = "YesNo", Section = "AI. Apply AI", OrderIndex = 19 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI-5", ItemContent = "Does the class diagram show the AI calling and processing packaged into separate classes/modules?", ItemType = "YesNo", Section = "AI. Apply AI", OrderIndex = 20 },
                    });
                }
                else // Round 3 - Rubric
                {
                    items.AddRange(new List<ChecklistItem>
                    {
                        // Product
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P1", ItemName = "Scope: Coverage of objectives", ItemContent = "Evaluation of problem solving, system size, and coverage of objectives.", ItemType = "Rubric", Section = "I. Product", OrderIndex = 1 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P2", ItemName = "Practical applicability", ItemContent = "Evaluation of production deployment, real users, and stakeholder interaction.", ItemType = "Rubric", Section = "I. Product", OrderIndex = 2 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P3", ItemName = "Innovation and Creativity", ItemContent = "Evaluation of new business models, UI/UX, and technical innovation.", ItemType = "Rubric", Section = "I. Product", OrderIndex = 3 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P4", ItemName = "UI/UX", ItemContent = "Evaluation of branding, layout, navigation, and color system.", ItemType = "Rubric", Section = "I. Product", OrderIndex = 4 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P5", ItemName = "Technology choices for Software Architecture", ItemContent = "Evaluation of external services, frontend, and backend technology stacks.", ItemType = "Rubric", Section = "I. Product", OrderIndex = 5 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P6", ItemName = "Application of computing knowledge for Implementation", ItemContent = "Evaluation of source code quality, patterns, and programming techniques.", ItemType = "Rubric", Section = "I. Product", OrderIndex = 6 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P7", ItemName = "Complexity of algorithm/internal processing", ItemContent = "Evaluation of algorithm choice and implementation complexity.", ItemType = "Rubric", Section = "I. Product", OrderIndex = 7 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "P8", ItemName = "Technology choices for Deployment & Maintenance", ItemContent = "Evaluation of Git usage, CI/CD, and cloud deployment.", ItemType = "Rubric", Section = "I. Product", OrderIndex = 8 },

                        // Document
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D1", ItemName = "User requirement and System Requirement", ItemContent = "Evaluation of SRS documentation, diagrams, and formatting.", ItemType = "Rubric", Section = "II. Document", OrderIndex = 9 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D2", ItemName = "Architecture Design Document", ItemContent = "Evaluation of technology strategy and system architecture diagrams.", ItemType = "Rubric", Section = "II. Document", OrderIndex = 10 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D3", ItemName = "Detail Design Document", ItemContent = "Evaluation of class design, database design, and logical ERDs.", ItemType = "Rubric", Section = "II. Document", OrderIndex = 11 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D4", ItemName = "Testing Document", ItemContent = "Evaluation of test plans, test cases, and test reports.", ItemType = "Rubric", Section = "II. Document", OrderIndex = 12 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "D5", ItemName = "System Deployment and Delivery Package", ItemContent = "Evaluation of delivery package, installability, and user manuals.", ItemType = "Rubric", Section = "II. Document", OrderIndex = 13 },

                        // Presentation
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "Pr1", ItemName = "Presentation Skills", ItemContent = "Evaluation of presentation design, flow, and time management.", ItemType = "Rubric", Section = "III. Presentation", OrderIndex = 14 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "Pr2", ItemName = "Demonstration", ItemContent = "Evaluation of system demonstration and evidence of results.", ItemType = "Rubric", Section = "III. Presentation", OrderIndex = 15 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "Pr3", ItemName = "Q&A", ItemContent = "Evaluation of answer completeness and deeper understanding.", ItemType = "Rubric", Section = "III. Presentation", OrderIndex = 16 },

                        // AI
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI1", ItemName = "Appropriateness of AI solution selection", ItemContent = "Evaluation of AI choice relevance and problem solving.", ItemType = "Rubric", Section = "IV. AI", OrderIndex = 17 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI2", ItemName = "Quality of AI model building and training", ItemContent = "Evaluation of AI data selection and model training quality.", ItemType = "Rubric", Section = "IV. AI", OrderIndex = 18 },
                        new ChecklistItem { ChecklistID = checklist.ChecklistID, ItemCode = "AI3", ItemName = "AI integration into software system", ItemContent = "Evaluation of AI component integration and architecture.", ItemType = "Rubric", Section = "IV. AI", OrderIndex = 19 },
                    });

                    // Add RubricDescriptions for each item
                    foreach (var item in items)
                    {
                        item.RubricDescriptions.Add(new RubricDescription { GradeLevel = "Excellent", Description = "Outstanding performance, exceeding all standard requirements with high quality." });
                        item.RubricDescriptions.Add(new RubricDescription { GradeLevel = "Good", Description = "Good performance, meeting all requirements with minor rooms for improvement." });
                        item.RubricDescriptions.Add(new RubricDescription { GradeLevel = "Acceptable", Description = "Minimum acceptable performance, meeting basic requirements." });
                        item.RubricDescriptions.Add(new RubricDescription { GradeLevel = "Fail", Description = "Performance below minimum standards or missing critical components." });
                    }
                }

                _context.ChecklistItems.AddRange(items);
            }

            // Submission Requirements - Check if already exists
            if (!await _context.SubmissionRequirements.AnyAsync(sr => sr.ReviewRoundID == round.ReviewRoundID))
            {
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
        }

        await _context.SaveChangesAsync();
    }
}
