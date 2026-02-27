using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IProjectGroupRepository
{
    Task<ProjectGroup?> GetByIdAsync(int groupId);
    Task<IEnumerable<ProjectGroup>> GetByProjectIdAsync(int projectId);
    Task AddAsync(ProjectGroup group);
    Task AddMemberAsync(GroupMember member);
    Task SaveChangesAsync();
}
