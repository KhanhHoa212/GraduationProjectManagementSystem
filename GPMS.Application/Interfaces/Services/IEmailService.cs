using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
