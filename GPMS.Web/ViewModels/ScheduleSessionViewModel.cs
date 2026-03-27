using System;
using System.Collections.Generic;
using GPMS.Application.DTOs;
using GPMS.Domain.Entities;

namespace GPMS.Web.ViewModels;

public class ScheduleSessionViewModel
{
    public int RoundId { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public List<GroupSessionListItemViewModel> Groups { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
}

public class GroupSessionListItemViewModel
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public int? SessionId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int? RoomId { get; set; }
    public string? MeetLink { get; set; }
    public string? Notes { get; set; }
    public bool IsOnline => !string.IsNullOrEmpty(MeetLink) || RoomId == null;
}
