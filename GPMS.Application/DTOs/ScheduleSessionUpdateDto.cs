using System;

namespace GPMS.Application.DTOs;

public class ScheduleSessionUpdateDto
{
    public int? SessionID { get; set; }
    public int GroupID { get; set; }
    public int ReviewRoundID { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int? RoomID { get; set; }
    public string? MeetLink { get; set; }
    public string? Notes { get; set; }
    public bool IsOnline { get; set; }
}
