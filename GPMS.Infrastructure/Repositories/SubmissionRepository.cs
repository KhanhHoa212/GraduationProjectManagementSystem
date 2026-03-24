using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly GpmsDbContext _context;
    public SubmissionRepository(GpmsDbContext context) => _context = context;

    public async Task<Submission?> GetByIdAsync(int submissionId) => await _context.Submissions.FindAsync(submissionId);
    public async Task<SubmissionRequirement?> GetRequirementByIdAsync(int requirementId) => 
        await _context.SubmissionRequirements.Include(r => r.ReviewRound).FirstOrDefaultAsync(r => r.RequirementID == requirementId);
    public async Task<IEnumerable<Submission>> GetByGroupAndRequirementAsync(int groupId, int requirementId) => 
        await _context.Submissions.Where(s => s.GroupID == groupId && s.RequirementID == requirementId).ToListAsync();

    public async Task<IEnumerable<Submission>> GetByGroupIdsAsync(IEnumerable<int> groupIds) =>
        await _context.Submissions.Where(s => groupIds.Contains(s.GroupID)).ToListAsync();

    public async Task AddAsync(Submission submission) => await _context.Submissions.AddAsync(submission);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<(SubmissionRequirement Requirement, Submission? Submission)>> GetActiveSubmissionsByStudentAsync(string studentId)
    {
        // 1. Find the student's active group
        var group = await _context.ProjectGroups
            .Include(g => g.GroupMembers)
            .Where(g => g.GroupMembers.Any(m => m.UserID == studentId))
            .FirstOrDefaultAsync();

        if (group == null)
            return Enumerable.Empty<(SubmissionRequirement Requirement, Submission? Submission)>();

        // 2. Fetch all requirements for the project's active review round, or just all requirements linked to the project's semester?
        // In this system, requirements are linked to ReviewRound. Let's fetch all requirements that are relevant to this group.
        // For simplicity based on the current domain model, we find all ReviewRounds for this project's semester, then all requirements.
        var project = await _context.Projects.FindAsync(group.ProjectID);
        if (project == null) return Enumerable.Empty<(SubmissionRequirement Requirement, Submission? Submission)>();

        var reviewRounds = await _context.ReviewRounds
            .Where(r => r.SemesterID == project.SemesterID)
            .OrderBy(r => r.RoundNumber)
            .Select(r => r.ReviewRoundID)
            .ToListAsync();

        var requirements = await _context.SubmissionRequirements
            .Include(req => req.ReviewRound)
            .Where(req => reviewRounds.Contains(req.ReviewRoundID))
            .OrderBy(req => req.Deadline)
            .ToListAsync();

        var submissions = await _context.Submissions
            .Where(s => s.GroupID == group.GroupID)
            .ToListAsync();

        var result = new List<(SubmissionRequirement Requirement, Submission? Submission)>();
        foreach (var req in requirements)
        {
            var sub = submissions.FirstOrDefault(s => s.RequirementID == req.RequirementID);
            result.Add((Requirement: req, Submission: sub));
        }

        return result;
    }

    public async Task<IEnumerable<(SubmissionRequirement Requirement, Submission? Submission)>> GetAllSubmissionsByStudentAsync(string studentId)
    {
        var group = await _context.ProjectGroups
            .Include(g => g.GroupMembers)
            .Where(g => g.GroupMembers.Any(m => m.UserID == studentId))
            .FirstOrDefaultAsync();

        if (group == null)
            return Enumerable.Empty<(SubmissionRequirement Requirement, Submission? Submission)>();

        var project = await _context.Projects.FindAsync(group.ProjectID);
        if (project == null) return Enumerable.Empty<(SubmissionRequirement Requirement, Submission? Submission)>();

        var reviewRounds = await _context.ReviewRounds
            .Where(r => r.SemesterID == project.SemesterID)
            .OrderBy(r => r.RoundNumber)
            .Select(r => r.ReviewRoundID)
            .ToListAsync();

        var requirements = await _context.SubmissionRequirements
            .Include(req => req.ReviewRound)
            .Where(req => reviewRounds.Contains(req.ReviewRoundID))
            .OrderBy(req => req.Deadline)
            .ToListAsync();

        var submissions = await _context.Submissions
            .Where(s => s.GroupID == group.GroupID)
            .ToListAsync();

        var result = new List<(SubmissionRequirement Requirement, Submission? Submission)>();
        foreach (var req in requirements)
        {
            var sub = submissions.FirstOrDefault(s => s.RequirementID == req.RequirementID);
            result.Add((Requirement: req, Submission: sub));
        }

        return result;
    }
}
