using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class ProjectGroupRepository : IProjectGroupRepository
{
    private readonly GpmsDbContext _context;
    public ProjectGroupRepository(GpmsDbContext context) => _context = context;

    public async Task<ProjectGroup?> GetByIdAsync(int groupId) =>
        await _context.ProjectGroups
            .Include(pg => pg.Project)
                .ThenInclude(p => p.Semester)
            .Include(pg => pg.Project)
                .ThenInclude(p => p.ProjectSupervisors)
                    .ThenInclude(ps => ps.Lecturer)
            .Include(pg => pg.GroupMembers)
                .ThenInclude(m => m.User)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.Feedback)
                    .ThenInclude(f => f!.FeedbackApproval)
            .FirstOrDefaultAsync(pg => pg.GroupID == groupId);

    public async Task<IEnumerable<ProjectGroup>> GetByProjectIdAsync(int projectId) =>
        await _context.ProjectGroups.Where(pg => pg.ProjectID == projectId).ToListAsync();

    public async Task<IEnumerable<ProjectGroup>> GetBySupervisorAsync(string supervisorId) =>
        await _context.ProjectGroups
            .Include(pg => pg.Project)
                .ThenInclude(p => p.Semester)
            .Include(pg => pg.Project)
                .ThenInclude(p => p.ProjectSupervisors)
            .Include(pg => pg.GroupMembers)
                .ThenInclude(m => m.User)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.Feedback)
                    .ThenInclude(f => f!.FeedbackApproval)
            .Where(pg => pg.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId))
            .ToListAsync();

    public async Task<ProjectGroup?> GetByProjectIdWithMembersAsync(int projectId) =>
        await _context.ProjectGroups
            .Include(g => g.GroupMembers)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.ProjectID == projectId);

    public async Task<GroupMember?> GetMemberAsync(int groupId, string userId) =>
        await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupID == groupId && m.UserID == userId);

    public async Task AddAsync(ProjectGroup group) =>
        await _context.ProjectGroups.AddAsync(group);

    public async Task AddMemberAsync(GroupMember member) =>
        await _context.GroupMembers.AddAsync(member);

    public Task RemoveMemberAsync(GroupMember member)
    {
        _context.GroupMembers.Remove(member);
        return Task.CompletedTask;
    }

    public Task UpdateMemberAsync(GroupMember member)
    {
        _context.GroupMembers.Update(member);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ProjectGroup>> GetAllWithDetailsAsync() =>
        await _context.ProjectGroups
            .Include(g => g.Project)
                .ThenInclude(p => p.ProjectSupervisors)
                    .ThenInclude(ps => ps.Lecturer)
            .Include(g => g.GroupMembers)
                .ThenInclude(m => m.User)
            .ToListAsync();

    public async Task<bool> IsUserInAnyGroupAsync(string userId) =>
        await _context.GroupMembers.AnyAsync(m => m.UserID == userId);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
