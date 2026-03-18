using GPMS.Application.DTOs;
using GPMS.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync(string? search = null, string? role = null, UserStatus? status = null);
    Task<UserDto?> GetUserByIdAsync(string id);
    Task CreateUserAsync(CreateUserDto dto);
    Task UpdateUserAsync(UpdateUserDto dto);
    Task ToggleUserStatusAsync(string id);
    Task<(int total, int students, int lecturers, int admins, int hods)> GetUserCountsAsync();
}
