using AutoMapper;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class ProjectGroupService : IProjectGroupService
{
    private readonly IProjectGroupRepository _groupRepository;
    private readonly IMapper _mapper;

    public ProjectGroupService(IProjectGroupRepository groupRepository, IMapper mapper)
    {
        _groupRepository = groupRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProjectGroupDto>> GetAllGroupsAsync(string? search = null, string? status = null, string? supervisor = null)
    {
        var groups = await _groupRepository.GetAllWithDetailsAsync();

        // Apply filters in-memory for now (or move to repository if performance becomes an issue)
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            groups = groups.Where(g => 
                g.GroupName.ToLower().Contains(search) || 
                g.Project.ProjectName.ToLower().Contains(search) || 
                g.Project.ProjectCode.ToLower().Contains(search) ||
                g.GroupMembers.Any(m => m.User.FullName.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "All Projects")
        {
            if (Enum.TryParse<ProjectStatus>(status, out var projectStatus))
            {
                groups = groups.Where(g => g.Project.Status == projectStatus);
            }
        }

        if (!string.IsNullOrWhiteSpace(supervisor) && supervisor != "All Supervisors")
        {
            groups = groups.Where(g => 
                g.Project.ProjectSupervisors.Any(ps => ps.Role == ProjectRole.Main && ps.Lecturer.FullName == supervisor));
        }

        return _mapper.Map<IEnumerable<ProjectGroupDto>>(groups);
    }


    public async Task<ProjectGroupDetailDto?> GetGroupDetailAsync(int groupId)
    {
        // For simplicity, we fetch all and filter to get the details loaded by GetAllWithDetailsAsync
        var groups = await _groupRepository.GetAllWithDetailsAsync();
        var detailedGroup = groups.FirstOrDefault(g => g.GroupID == groupId);
        
        return _mapper.Map<ProjectGroupDetailDto>(detailedGroup);
    }

    public async Task<bool> DeleteGroupAsync(int groupId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group == null) return false;

        // Implementation for deletion if needed
        return true; 
    }
}
