using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int projectId);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetBySemesterAsync(int semesterId);

    // With eager loading (for HOD views)
    Task<IEnumerable<Project>> GetAllWithDetailsAsync();
    Task<IEnumerable<Project>> GetBySemesterWithDetailsAsync(int semesterId);
    Task<Project?> GetDetailAsync(int projectId);

    Task<Project?> GetProjectByStudentIdAsync(string studentId);

    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
    Task SaveChangesAsync();
}
