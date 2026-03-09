using GPMS.Application.DTOs;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task<AuthResultDto> GoogleLoginOrRegisterAsync(string email, string fullName, string? picture);
    Task<AuthResultDto> ForgotPasswordAsync(string email);
    Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto dto);
}
