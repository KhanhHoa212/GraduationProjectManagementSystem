using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class LecturerExpertise
{
    public string LecturerID { get; set; } = string.Empty;
    public int ExpertiseID { get; set; }
    public LecturerLevel Level { get; set; } = LecturerLevel.Basic;
    public bool IsPrimary { get; set; } = false;

    // Navigation
    public virtual User Lecturer { get; set; } = null!;
    public virtual ExpertiseArea Expertise { get; set; } = null!;
}
