using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class UserRole
{
    public int UserRoleID { get; set; }
    public string UserID { get; set; } = string.Empty;
    public RoleName RoleName { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User User { get; set; } = null!;
}
