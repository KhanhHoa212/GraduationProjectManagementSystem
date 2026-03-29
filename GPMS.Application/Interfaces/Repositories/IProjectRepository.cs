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
    Task<IEnumerable<Project>> GetDashboardProjectsAsync(int semesterId, int count);
    Task<IEnumerable<Project>> GetFilteredProjectsAsync(int? semesterId, string? status, string? search, string? majorName);
    Task<IEnumerable<Project>> GetSupervisorAssignmentProjectsAsync(int? semesterId);
    Task<IEnumerable<Project>> GetProgressProjectsAsync(int semesterId);
    Task<(int total, int withGroup, int missingSupervisor, int missingMembers, int draftCount, int activeCount, int completedCount)> GetDashboardStatsBySemesterAsync(int semesterId);
    Task<Project?> GetDetailAsync(int projectId);

    Task<Project?> GetProjectByStudentIdAsync(string studentId);

    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
    Task SaveChangesAsync();
}
