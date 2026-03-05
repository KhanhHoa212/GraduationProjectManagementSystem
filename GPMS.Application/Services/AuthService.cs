using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(dto.Identifier);
        if (user == null)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid login attempt." };
        }

        var credential = user.UserCredentials.FirstOrDefault(c => string.IsNullOrEmpty(c.ExternalProviderID) && !string.IsNullOrWhiteSpace(c.PasswordHash));
        if (credential == null || !BCrypt.Net.BCrypt.Verify(dto.Password, credential.PasswordHash))
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid login attempt." };
        }

        if (user.Status == UserStatus.Inactive)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Your account is deactivated. Please contact the administrator." };
        }

        return new AuthResultDto
        {
            Success = true,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResultDto> GoogleLoginOrRegisterAsync(string email, string fullName, string? picture)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        
        bool isAllowedDomain = email.EndsWith("@fe.edu.vn", StringComparison.OrdinalIgnoreCase) || 
                              email.EndsWith("@fpt.edu.vn", StringComparison.OrdinalIgnoreCase);

        if (!isAllowedDomain && existingUser == null)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Only @fe.edu.vn and @fpt.edu.vn accounts or pre-registered personal Gmails are allowed to login." };
        }

        User dbUser;
        if (existingUser == null)
        {
            dbUser = new User
            {
                UserID = Guid.NewGuid().ToString().Substring(0, 20),
                Email = email,
                FullName = fullName,
                AvatarUrl = picture,
                Status = UserStatus.Active
            };
            dbUser.UserRoles.Add(new UserRole { RoleName = RoleName.Student, UserID = dbUser.UserID });
            await _userRepository.AddAsync(dbUser);
        }
        else
        {
            dbUser = existingUser;
            dbUser.FullName = fullName;
            dbUser.AvatarUrl = picture ?? dbUser.AvatarUrl;
            await _userRepository.UpdateAsync(dbUser);
        }

        await _userRepository.SaveChangesAsync();

        if (dbUser.Status == UserStatus.Inactive)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Your account is deactivated. Please contact the administrator." };
        }

        return new AuthResultDto
        {
            Success = true,
            User = MapToDto(dbUser)
        };
    }

    public async Task<AuthResultDto> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user != null)
        {
            var credential = user.UserCredentials.FirstOrDefault(c => c.AuthProvider == AuthProvider.Internal || string.IsNullOrEmpty(c.ExternalProviderID));
            if (credential != null)
            {
                var token = Guid.NewGuid().ToString("N");
                credential.PasswordResetToken = token;
                credential.PasswordResetExpiry = DateTime.UtcNow.AddHours(2);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                
                // Usually we'd send an email here.
                // For now we just return the token if we wanted to mock it.
            }
        }

        // Always return success to avoid leaking existence of user.
        return new AuthResultDto { Success = true };
    }

    public async Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.UserCredentials.Any(c => 
            c.PasswordResetToken == dto.Token && 
            c.PasswordResetExpiry.HasValue && 
            c.PasswordResetExpiry.Value > DateTime.UtcNow));

        if (user == null)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid or expired link." };
        }

        var credential = user.UserCredentials.First(c => c.PasswordResetToken == dto.Token);
        credential.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        credential.PasswordResetToken = null;
        credential.PasswordResetExpiry = null;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return new AuthResultDto { Success = true };
    }

    private UserDto MapToDto(User u)
    {
        return new UserDto
        {
            UserID = u.UserID,
            Username = u.Username,
            Email = u.Email,
            FullName = u.FullName,
            Phone = u.Phone,
            Status = u.Status,
            Roles = u.UserRoles.Select(r => r.RoleName.ToString()).ToList()
        };
    }
}
