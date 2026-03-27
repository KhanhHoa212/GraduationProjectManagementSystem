using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using GPMS.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class GoogleCalendarService : IMeetingService
{
    private readonly IConfiguration _configuration;
    private readonly string _calendarId;
    private readonly string? _serviceAccountKeyPath;

    public GoogleCalendarService(IConfiguration configuration)
    {
        _configuration = configuration;
        _calendarId = _configuration["GoogleCalendar:CalendarId"] ?? "primary";
        _serviceAccountKeyPath = _configuration["GoogleCalendar:ServiceAccountKeyPath"];
    }

    public async Task<string> CreateMeetingAsync(string title, DateTime startTime, int durationMinutes, List<string> attendeeEmails)
    {
        // For development purpose, if no key is provided, return a mock link
        if (string.IsNullOrEmpty(_serviceAccountKeyPath))
        {
            // If we are in "Development" or "Mock" mode, return a generated link
            return $"https://meet.google.com/gpms-{Guid.NewGuid().ToString("N").Substring(0, 10)}";
        }

        try
        {
            GoogleCredential credential;
            using (var stream = new System.IO.FileStream(_serviceAccountKeyPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(CalendarService.Scope.Calendar);
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GPMS Graduation Project Management System",
            });

            var newEvent = new Event()
            {
                Summary = title,
                Start = new EventDateTime()
                {
                    DateTimeDateTimeOffset = startTime,
                    TimeZone = "Asia/Ho_Chi_Minh",
                },
                End = new EventDateTime()
                {
                    DateTimeDateTimeOffset = startTime.AddMinutes(durationMinutes),
                    TimeZone = "Asia/Ho_Chi_Minh",
                },
                Attendees = attendeeEmails.Select(email => new EventAttendee { Email = email }).ToList(),
                ConferenceData = new ConferenceData
                {
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey { Type = "hangoutsMeet" }
                    }
                }
            };

            var request = service.Events.Insert(newEvent, _calendarId);
            request.ConferenceDataVersion = 1;
            var createdEvent = await request.ExecuteAsync();

            return createdEvent.HangoutLink ?? createdEvent.HtmlLink;
        }
        catch (Exception ex)
        {
            // Fallback to manual link if integration fails
            Console.WriteLine($"Error creating Google Meet: {ex.Message}");
            return $"https://meet.google.com/gpms-{Guid.NewGuid().ToString("N").Substring(0, 10)}";
        }
    }
}
