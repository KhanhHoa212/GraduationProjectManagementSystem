using GPMS.Domain.Enums;
using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs;

public class ProjectGroupDto
{
    public int GroupID { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int ProjectID { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string? SupervisorName { get; set; }
    public ProjectStatus ProjectStatus { get; set; }
    public int MemberCount { get; set; }
    public List<string> MemberNames { get; set; } = new();
    public string? CurrentReviewPhase { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProjectGroupDetailDto : ProjectGroupDto
{
    public List<GroupMemberDto> Members { get; set; } = new();
}

public class GroupMemberDto
{
    public string UserID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public GroupRole RoleInGroup { get; set; }
    public DateTime JoinedAt { get; set; }
}
