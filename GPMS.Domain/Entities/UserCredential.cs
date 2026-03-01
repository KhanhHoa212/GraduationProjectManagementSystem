using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class UserCredential
{
    public int CredentialID { get; set; }
    public string UserID { get; set; } = string.Empty;
    public AuthProvider AuthProvider { get; set; }
    public string? PasswordHash { get; set; }
    public string? ExternalProviderID { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
}
