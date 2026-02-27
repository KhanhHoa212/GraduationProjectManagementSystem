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

    public async Task<ProjectGroup?> GetByIdAsync(int groupId) => await _context.ProjectGroups.FindAsync(groupId);
    public async Task<IEnumerable<ProjectGroup>> GetByProjectIdAsync(int projectId) => 
        await _context.ProjectGroups.Where(pg => pg.ProjectID == projectId).ToListAsync();
    public async Task AddAsync(ProjectGroup group) => await _context.ProjectGroups.AddAsync(group);
    public async Task AddMemberAsync(GroupMember member) => await _context.GroupMembers.AddAsync(member);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
