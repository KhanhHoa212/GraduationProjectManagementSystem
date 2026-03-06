using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using GPMS.Application.Interfaces.Services;

namespace GPMS.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var from = _settings.From;
                var smtpServer = _settings.SmtpServer;
                var port = _settings.Port;
                var username = _settings.Username;
                var password = _settings.Password;

                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new Exception("Email settings are not fully configured.");
                }

                var msg = new MailMessage(from, toEmail, subject, body);
                msg.IsBodyHtml = true;

                using var client = new SmtpClient(smtpServer, port)
                {
                    Credentials = new System.Net.NetworkCredential(username, password),
                    EnableSsl = true
                };

                await client.SendMailAsync(msg);
            }
            catch (Exception ex)
            {
                // In a real app, log this exception
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw; // Rethrow to let the caller handle it if needed
            }
        }
    }
}
