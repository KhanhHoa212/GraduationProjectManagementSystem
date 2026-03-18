using AutoMapper;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(string? search = null, string? role = null, UserStatus? status = null)
    {
        var users = await _userRepository.GetAllAsync();
        if (status.HasValue)
        {
            users = users.Where(u => u.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(role) && role != "All Roles")
        {
            users = users.Where(u => u.UserRoles.Any(r => r.RoleName.ToString().Equals(role, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            users = users.Where(u => 
                u.FullName.ToLower().Contains(search) || 
                (u.Email != null && u.Email.ToLower().Contains(search)) || 
                u.UserID.ToLower().Contains(search));
        }

        // Sử dụng Mapper thay thế cho đoạn Manual Mapping thủ công 
        return _mapper.Map<IEnumerable<UserDto>>(users);

    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return _mapper.Map<UserDto>(user);
    }

    public async Task CreateUserAsync(CreateUserDto dto)
    {
        var newUser = new User
        {
            UserID = string.IsNullOrWhiteSpace(dto.UserID) ? Guid.NewGuid().ToString().Substring(0, 20) : dto.UserID,
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            Phone = dto.Phone,
            Status = dto.Status
        };

        // Check if UserID already exists
        if (await _userRepository.ExistsAsync(newUser.UserID))
        {
            throw new InvalidOperationException("User ID already exists.");
        }

        // Check if Email already exists
        if (!string.IsNullOrEmpty(newUser.Email))
        {
            var userByEmail = await _userRepository.GetByEmailAsync(newUser.Email);
            if (userByEmail != null)
            {
                throw new InvalidOperationException("Email address already exists.");
            }
        }

        // Check if Username already exists
        if (!string.IsNullOrEmpty(newUser.Username))
        {
            var userByUsername = await _userRepository.GetByUsernameOrEmailAsync(newUser.Username);
            if (userByUsername != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }
        }

        if (dto.Role == "Admin")
            newUser.UserRoles.Add(new UserRole { RoleName = RoleName.Admin, UserID = newUser.UserID });
        else if (dto.Role == "HeadOfDept")
            newUser.UserRoles.Add(new UserRole { RoleName = RoleName.HeadOfDept, UserID = newUser.UserID });
        else if (dto.Role == "Lecturer")
            newUser.UserRoles.Add(new UserRole { RoleName = RoleName.Lecturer, UserID = newUser.UserID });
        else
            newUser.UserRoles.Add(new UserRole { RoleName = RoleName.Student, UserID = newUser.UserID });

        // Create default credential so "Forgot Password" works
        newUser.UserCredentials.Add(new UserCredential
        {
            UserID = newUser.UserID,
            AuthProvider = AuthProvider.Internal,
            PasswordHash = null // User will need to use Forgot Password to set it
        });

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserID);
        if (user == null) return;

        // Check if Email already exists for another user
        if (!string.IsNullOrEmpty(dto.Email))
        {
            var userByEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (userByEmail != null && userByEmail.UserID != dto.UserID)
            {
                throw new InvalidOperationException("Email address already exists.");
            }
        }

        // Check if Username already exists for another user
        if (!string.IsNullOrEmpty(dto.Username))
        {
            var userByUsername = await _userRepository.GetByUsernameOrEmailAsync(dto.Username);
            if (userByUsername != null && userByUsername.UserID != dto.UserID)
            {
                throw new InvalidOperationException("Username already exists.");
            }
        }

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.Phone = dto.Phone;
        user.Status = dto.Status;

        var wasAdmin = user.UserRoles.Any(r => r.RoleName == RoleName.Admin);
        if (wasAdmin && dto.Role != "Admin")
        {
            // Usually we might throw an exception or return a result object
            // For now, let's just stick to the current logic which was handled in controller
            // But better to at least check here
            return; 
        }

        user.UserRoles.Clear();
        if (dto.Role == "Admin")
            user.UserRoles.Add(new UserRole { RoleName = RoleName.Admin, UserID = user.UserID });
        else if (dto.Role == "HeadOfDept")
            user.UserRoles.Add(new UserRole { RoleName = RoleName.HeadOfDept, UserID = user.UserID });
        else if (dto.Role == "Lecturer")
            user.UserRoles.Add(new UserRole { RoleName = RoleName.Lecturer, UserID = user.UserID });
        else
            user.UserRoles.Add(new UserRole { RoleName = RoleName.Student, UserID = user.UserID });

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ToggleUserStatusAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return;

        var isAdmin = user.UserRoles.Any(r => r.RoleName == RoleName.Admin);
        if (isAdmin) return;

        user.Status = user.Status == UserStatus.Active ? UserStatus.Inactive : UserStatus.Active;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<(int total, int students, int lecturers, int admins, int hods)> GetUserCountsAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var list = users.ToList();
        int total = list.Count;
        int students  = list.Count(u => u.UserRoles.Any(r => r.RoleName == RoleName.Student));
        int lecturers = list.Count(u => u.UserRoles.Any(r => r.RoleName == RoleName.Lecturer));
        int admins    = list.Count(u => u.UserRoles.Any(r => r.RoleName == RoleName.Admin));
        int hods      = list.Count(u => u.UserRoles.Any(r => r.RoleName == RoleName.HeadOfDept));
        return (total, students, lecturers, admins, hods);
    }
}
