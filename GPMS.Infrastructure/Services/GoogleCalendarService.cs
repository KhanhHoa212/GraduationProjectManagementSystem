using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using GPMS.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Services;

public class GoogleCalendarService : IMeetingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(IConfiguration configuration, ILogger<GoogleCalendarService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> CreateOnlineMeetingAsync(string summary, string description, DateTime startTime, DateTime endTime)
    {
        try
        {
            var credentialPath = _configuration["GoogleCalendar:CredentialFile"];
            if (string.IsNullOrEmpty(credentialPath) || !System.IO.File.Exists(credentialPath))
            {
                _logger.LogWarning("Google Calendar credentials not found in GoogleCalendar:CredentialFile. Generating a fallback mock Meet link.");
                // Provide a visually distinct mock link so users know it's a fallback
                return $"https://meet.google.com/mock-{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 4)}";
            }

            GoogleCredential credential;
            using (var stream = new System.IO.FileStream(credentialPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(CalendarService.Scope.CalendarEvents);
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GPMS",
            });

            Event newEvent = new Event()
            {
                Summary = summary,
                Description = description,
                Start = new EventDateTime() { DateTimeDateTimeOffset = startTime, TimeZone = "Asia/Ho_Chi_Minh" },
                End = new EventDateTime() { DateTimeDateTimeOffset = endTime, TimeZone = "Asia/Ho_Chi_Minh" },
                ConferenceData = new ConferenceData
                {
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey { Type = "hangoutsMeet" }
                    }
                }
            };

            var request = service.Events.Insert(newEvent, "primary");
            request.ConferenceDataVersion = 1;
            var createdEvent = await request.ExecuteAsync();

            return createdEvent.HangoutLink ?? $"https://meet.google.com/mock-{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 4)}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Google Meet link.");
            return $"https://meet.google.com/mock-{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 4)}";
        }
    }
}
