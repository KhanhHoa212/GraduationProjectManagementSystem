using GPMS.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IProjectGroupService
{
    Task<IEnumerable<ProjectGroupDto>> GetAllGroupsAsync(string? search = null, string? status = null, string? supervisor = null);
    Task<ProjectGroupDetailDto?> GetGroupDetailAsync(int groupId);
    Task<bool> DeleteGroupAsync(int groupId);
}
