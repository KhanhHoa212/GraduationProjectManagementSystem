using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class GroupMember
{
    public int GroupID { get; set; }
    public string UserID { get; set; } = string.Empty;
    public GroupRole RoleInGroup { get; set; } = GroupRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ProjectGroup Group { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
