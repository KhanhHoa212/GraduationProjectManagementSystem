using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int projectId);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetBySemesterAsync(int semesterId);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task SaveChangesAsync();
}
