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
    public async Task<IEnumerable<Submission>> GetByGroupAndRequirementAsync(int groupId, int requirementId) => 
        await _context.Submissions.Where(s => s.GroupID == groupId && s.RequirementID == requirementId).ToListAsync();
    public async Task AddAsync(Submission submission) => await _context.Submissions.AddAsync(submission);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
