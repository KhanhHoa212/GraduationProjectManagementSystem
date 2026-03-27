using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IMeetingService
{
    /// <summary>
    /// Creates an online meeting (Google Meet) and returns the join link.
    /// </summary>
    /// <param name="title">Title of the meeting.</param>
    /// <param name="startTime">Start time of the meeting.</param>
    /// <param name="durationMinutes">Duration in minutes.</param>
    /// <param name="attendeeEmails">List of participant emails.</param>
    /// <returns>The hangout link (Google Meet link).</returns>
    Task<string> CreateMeetingAsync(string title, DateTime startTime, int durationMinutes, List<string> attendeeEmails);
}
