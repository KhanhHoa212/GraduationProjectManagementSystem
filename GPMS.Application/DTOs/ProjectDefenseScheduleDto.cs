using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs;

public class ProjectDefenseScheduleDto
{
    public int RoundNumber { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string? RoomName { get; set; }
    public string? Building { get; set; }
    public string? Notes { get; set; }
    public List<CommitteeMemberDto> CommitteeMembers { get; set; } = new();
}

public class CommitteeMemberDto
{
    public string FullName { get; set; } = string.Empty;
    public string UserID { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Chủ tịch, Phản biện, Hướng dẫn
    public string? AvatarUrl { get; set; }
}
