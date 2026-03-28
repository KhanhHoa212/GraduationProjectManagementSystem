using GPMS.Application.DTOs.Lecturer;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IProjectGroupRepository
{
    // Entity methods (write-path — needed for update/gate operations)
    Task<ProjectGroup?> GetByIdAsync(int groupId);
    Task<IEnumerable<ProjectGroup>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<ProjectGroup>> GetBySupervisorAsync(string supervisorId);
    Task<ProjectGroup?> GetByProjectIdWithMembersAsync(int projectId);
    Task<GroupMember?> GetMemberAsync(int groupId, string userId);
    Task AddAsync(ProjectGroup group);
    Task AddMemberAsync(GroupMember member);
    Task RemoveMemberAsync(GroupMember member);
    Task UpdateMemberAsync(GroupMember member);
    Task<IEnumerable<ProjectGroup>> GetAllWithDetailsAsync();
    Task<IEnumerable<ProjectGroup>> GetBySemesterWithDetailsAsync(int semesterId);
    Task<bool> IsUserInAnyGroupAsync(string userId);
    Task<bool> HasUserGraduatedAsync(string userId);
    Task<ReviewSessionInfo?> GetGroupDefenseSessionAsync(int groupId);
    Task<IEnumerable<ReviewSessionInfo>> GetGroupSchedulesAsync(int groupId);
    Task SaveChangesAsync();

    // DTO Projection methods (read-only — no entity tracking)
    Task<IEnumerable<ProjectGroupSummaryDto>> GetSummariesBySupervisorAsync(string supervisorId);
    Task<LecturerProjectGroupDetailDto?> GetDetailDtoAsync(int groupId);
}
