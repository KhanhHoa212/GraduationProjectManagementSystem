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

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
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

        if (dto.Role == "Lecturer")
        {
            newUser.UserRoles.Add(new UserRole { RoleName = RoleName.Lecturer, UserID = newUser.UserID });
        }
        else
        {
            newUser.UserRoles.Add(new UserRole { RoleName = RoleName.Student, UserID = newUser.UserID });
        }

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserID);
        if (user == null) return;

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
}
