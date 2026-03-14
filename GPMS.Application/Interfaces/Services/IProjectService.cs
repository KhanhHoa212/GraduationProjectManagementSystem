using GPMS.Application.DTOs;

namespace GPMS.Application.Interfaces.Services;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<IEnumerable<ProjectDto>> GetProjectsBySemesterAsync(int semesterId);
    Task<ProjectDetailDto?> GetProjectDetailAsync(int projectId);
    Task CreateProjectAsync(CreateProjectDto dto);
    Task UpdateProjectAsync(UpdateProjectDto dto);
    Task<(int total, int withGroup, int missingSupervisor, int missingMembers)> GetDashboardStatsAsync(int? semesterId = null);
    
    // Student Dashboard
    Task<ProjectDto?> GetProjectByStudentAsync(string studentId);
    Task<IEnumerable<SubmissionItemDto>> GetDashboardSubmissionsAsync(string studentId);
    Task<IEnumerable<DashboardFeedbackDto>> GetDashboardFeedbacksAsync(string studentId, int count = 5);

    // Member management
    Task<IEnumerable<StudentSearchDto>> SearchStudentsAsync(string query);
    Task<(bool success, string message)> AddMemberAsync(int projectId, string userId);
    Task<bool> RemoveMemberAsync(int projectId, string userId);
    Task<(bool success, string message)> UpdateMemberRoleAsync(int projectId, string userId, string role);
}
