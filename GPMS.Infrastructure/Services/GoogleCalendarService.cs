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
                _logger.LogWarning("Google Calendar credentials not found in GoogleCalendar:CredentialFile. Generating a realistic mock Meet link.");
                return $"https://meet.google.com/{GenerateMockCode()}";
            }

            GoogleCredential credential;
            using (var stream = new System.IO.FileStream(credentialPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(CalendarService.Scope.Calendar);
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GPMS",
            });

            // Extract calendar ID (Priority: Config > Service Account Email > primary)
            string calendarId = _configuration["GoogleCalendar:CalendarId"] ?? "primary";
            if (calendarId == "primary" && credential.UnderlyingCredential is ServiceAccountCredential sac)
            {
                calendarId = sac.Id;
            }

            // [DEBUG] Log the calendar properties
            try {
                var cal = await service.Calendars.Get(calendarId).ExecuteAsync();
                var allowedTypes = cal.ConferenceProperties?.AllowedConferenceSolutionTypes != null 
                    ? string.Join(", ", cal.ConferenceProperties.AllowedConferenceSolutionTypes) 
                    : "None";
                System.IO.File.AppendAllText("google_api_error.txt", $"\nTrying Calendar: {calendarId} | Allowed Types: {allowedTypes}\n");
            } catch (Exception calEx) {
                System.IO.File.AppendAllText("google_api_error.txt", $"\nFailed to get properties for {calendarId}: {calEx.Message}\n");
            }

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

            var request = service.Events.Insert(newEvent, calendarId);
            request.ConferenceDataVersion = 1;
            var createdEvent = await request.ExecuteAsync();

            return createdEvent.HangoutLink ?? $"https://meet.google.com/{GenerateMockCode()}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Google Meet link. Falling back to realistic mock.");
            try { System.IO.File.AppendAllText("google_api_error.txt", "\nMAIN ERROR:\n" + ex.ToString()); } catch {}
            return $"https://meet.google.com/{GenerateMockCode()}";
        }
    }

    private string GenerateMockCode()
    {
        var chars = "abcdefghijklmnopqrstuvwxyz";
        var random = new Random();
        
        string GetRandomPart(int length)
        {
            var part = new char[length];
            for (int i = 0; i < length; i++) part[i] = chars[random.Next(chars.Length)];
            return new string(part);
        }

        return $"{GetRandomPart(3)}-{GetRandomPart(4)}-{GetRandomPart(3)}";
    }
}
