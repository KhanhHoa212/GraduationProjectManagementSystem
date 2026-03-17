using AutoMapper;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository userRepository, 
        IMapper mapper, 
        IEmailService emailService, 
        IConfiguration config)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _emailService = emailService;
        _config = config;
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
            User = _mapper.Map<UserDto>(user)
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
            User = _mapper.Map<UserDto>(dbUser)
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
                var baseUrl = _config["AppSettings:BaseUrl"];
                var resetLink = $"{baseUrl}/Auth/ResetPassword?token={token}";
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                
                // Send email
                var subject = "[NO REPLY] Reset Your Password - GPMS";
                var body = $@"
                <h2>Reset Your Password</h2>

                <p>You requested to reset your password for the Graduation Project Management System.</p>

                <p>Please click the link below to reset your password. This link will expire in 2 hours.</p>

                <p>
                <a href='{resetLink}' 
                style='padding:10px 18px;background:#F27123;color:white;text-decoration:none;border-radius:6px'>
                Reset Password
                </a>
                </p>

                <p>If the button doesn't work, copy this link:</p>

                <p>{resetLink}</p>

                <p>If you did not request this, please ignore this email.</p>
                ";
                Console.WriteLine($"[DEBUG] Attempting to send email to {email}");
                await _emailService.SendEmailAsync(email, subject, body);
                Console.WriteLine($"[DEBUG] Email sent successfully to {email}");
            }
            else 
            {
                // Create credential if missing (for legacy users)
                var newCredential = new UserCredential
                {
                    UserID = user.UserID,
                    AuthProvider = AuthProvider.Internal,
                    PasswordResetToken = Guid.NewGuid().ToString("N"),
                    PasswordResetExpiry = DateTime.UtcNow.AddHours(2)
                };
                user.UserCredentials.Add(newCredential);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                
                Console.WriteLine($"[DEBUG] Created new credential for {email} and sending email");
                await _emailService.SendEmailAsync(email, "Reset Your Password - GPMS", $"Token: {newCredential.PasswordResetToken}");
            }
        }
        else 
        {
            Console.WriteLine($"[DEBUG] User not found with email {email}");
        }

        // Always return success to avoid leaking existence of user.
        return new AuthResultDto { Success = true };
    }

    public async Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userRepository.GetUserByResetTokenAsync(dto.Token);

        if (user == null)
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Invalid or expired link."
            };
        }

        var credential = user.UserCredentials.FirstOrDefault(c => c.PasswordResetToken == dto.Token);

        if (credential == null)
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Invalid reset token."
            };
        }

        credential.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        credential.PasswordResetToken = null;
        credential.PasswordResetExpiry = null;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return new AuthResultDto { Success = true };
    }
}
