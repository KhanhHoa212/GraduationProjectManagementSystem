using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IProjectGroupRepository groupRepository,
        IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        var projects = await _projectRepository.GetAllWithDetailsAsync();
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsBySemesterAsync(int semesterId)
    {
        var projects = await _projectRepository.GetBySemesterWithDetailsAsync(semesterId);
        return projects.Select(MapToDto);
    }

    public async Task<ProjectDetailDto?> GetProjectDetailAsync(int projectId)
    {
        var project = await _projectRepository.GetDetailAsync(projectId);
        if (project == null) return null;

        var mainSupervisor = project.ProjectSupervisors
            .FirstOrDefault(ps => ps.Role == ProjectRole.Main);

        var members = project.ProjectGroups
            .SelectMany(g => g.GroupMembers.Select(m => new ProjectMemberDto
            {
                UserID = m.UserID,
                FullName = m.User.FullName,
                RoleInGroup = m.RoleInGroup,
                GroupName = g.GroupName
            }))
            .ToList();

        return new ProjectDetailDto
        {
            ProjectID = project.ProjectID,
            ProjectCode = project.ProjectCode,
            ProjectName = project.ProjectName,
            Description = project.Description,
            SemesterID = project.SemesterID,
            SemesterCode = project.Semester?.SemesterCode,
            MajorID = project.MajorID,
            MajorName = project.Major?.MajorName,
            Status = project.Status,
            CreatedAt = project.CreatedAt,
            SupervisorID = mainSupervisor?.LecturerID,
            SupervisorName = mainSupervisor?.Lecturer?.FullName,
            Members = members
        };
    }

    public async Task CreateProjectAsync(CreateProjectDto dto)
    {
        var allProjects = await _projectRepository.GetAllAsync();
        var count = allProjects.Count();
        var code = $"GP-{dto.SemesterID}-{count + 1:D3}";

        var project = new Project
        {
            ProjectCode = code,
            ProjectName = dto.ProjectName,
            Description = dto.Description,
            SemesterID = dto.SemesterID,
            MajorID = dto.MajorID,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task UpdateProjectAsync(UpdateProjectDto dto)
    {
        var project = await _projectRepository.GetByIdAsync(dto.ProjectID);
        if (project == null) return;

        project.ProjectName = dto.ProjectName;
        project.Description = dto.Description;
        project.Status = dto.Status;

        await _projectRepository.UpdateAsync(project);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task<(int total, int withGroup, int missingSupervisor, int missingMembers)> GetDashboardStatsAsync(int? semesterId = null)
    {
        var projects = semesterId.HasValue
            ? (await _projectRepository.GetBySemesterWithDetailsAsync(semesterId.Value)).ToList()
            : (await _projectRepository.GetAllWithDetailsAsync()).ToList();

        var total = projects.Count;
        var withGroup = projects.Count(p => p.ProjectGroups.Any());
        var missingSupervisor = projects.Count(p => !p.ProjectSupervisors.Any());
        var missingMembers = projects.Count(p => p.ProjectGroups.Any(g => !g.GroupMembers.Any()));

        return (total, withGroup, missingSupervisor, missingMembers);
    }

    // ============================================================
    // Member Management
    // ============================================================

    public async Task<IEnumerable<StudentSearchDto>> SearchStudentsAsync(string query)
    {
        var students = await _userRepository.GetByRoleAsync(RoleName.Student);
        return students
            .Where(u => u.UserID.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || u.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || (u.Email != null && u.Email.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .Select(u => new StudentSearchDto
            {
                UserID = u.UserID,
                FullName = u.FullName,
                Email = u.Email
            })
            .Take(10)
            .ToList();
    }

    public async Task<(bool success, string message)> AddMemberAsync(int projectId, string userId)
    {
        var student = await _userRepository.GetByIdAsync(userId);
        if (student == null)
            return (false, "Student not found.");

        if (await _groupRepository.IsUserInAnyGroupAsync(userId))
            return (false, "Student is already assigned to a project group.");

        var group = await _groupRepository.GetByProjectIdWithMembersAsync(projectId);
        if (group == null)
        {
            // Auto-create default group
            group = new ProjectGroup
            {
                ProjectID = projectId,
                GroupName = "Group 1",
                CreatedAt = DateTime.UtcNow
            };
            await _groupRepository.AddAsync(group);
            await _groupRepository.SaveChangesAsync();
            group = await _groupRepository.GetByProjectIdWithMembersAsync(projectId);
            if (group == null) return (false, "Failed to create group.");
        }

        if (group.GroupMembers.Any(m => m.UserID == userId))
            return (false, "Student is already a member of this group.");

        var role = group.GroupMembers.Any() ? GroupRole.Member : GroupRole.Leader;

        await _groupRepository.AddMemberAsync(new GroupMember
        {
            GroupID = group.GroupID,
            UserID = userId,
            RoleInGroup = role,
            JoinedAt = DateTime.UtcNow
        });
        await _groupRepository.SaveChangesAsync();

        return (true, $"Student added as {role}.");
    }

    public async Task<bool> RemoveMemberAsync(int projectId, string userId)
    {
        var group = await _groupRepository.GetByProjectIdWithMembersAsync(projectId);
        if (group == null) return false;

        var member = group.GroupMembers.FirstOrDefault(m => m.UserID == userId);
        if (member == null) return false;

        await _groupRepository.RemoveMemberAsync(member);
        await _groupRepository.SaveChangesAsync();
        return true;
    }

    public async Task<(bool success, string message)> UpdateMemberRoleAsync(int projectId, string userId, string role)
    {
        if (!Enum.TryParse<GroupRole>(role, out var newRole))
            return (false, "Invalid role.");

        var group = await _groupRepository.GetByProjectIdWithMembersAsync(projectId);
        if (group == null) return (false, "Group not found.");

        var member = group.GroupMembers.FirstOrDefault(m => m.UserID == userId);
        if (member == null) return (false, "Member not found.");

        if (newRole == GroupRole.Leader)
        {
            var existingLeader = group.GroupMembers
                .FirstOrDefault(m => m.RoleInGroup == GroupRole.Leader && m.UserID != userId);
            if (existingLeader != null)
                return (false, $"There is already a Leader in this group. Please demote them first.");
        }

        member.RoleInGroup = newRole;
        await _groupRepository.UpdateMemberAsync(member);
        await _groupRepository.SaveChangesAsync();

        return (true, "Role updated successfully.");
    }

    // ============================================================
    private static ProjectDto MapToDto(Project p)
    {
        var mainSupervisor = p.ProjectSupervisors
            .FirstOrDefault(ps => ps.Role == ProjectRole.Main);

        return new ProjectDto
        {
            ProjectID = p.ProjectID,
            ProjectCode = p.ProjectCode,
            ProjectName = p.ProjectName,
            MajorName = p.Major?.MajorName,
            SemesterCode = p.Semester?.SemesterCode,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            SupervisorID = mainSupervisor?.LecturerID,
            SupervisorName = mainSupervisor?.Lecturer?.FullName,
            GroupCount = p.ProjectGroups.Count
        };
    }
}
