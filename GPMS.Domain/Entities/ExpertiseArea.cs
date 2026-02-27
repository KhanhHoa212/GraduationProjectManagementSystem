namespace GPMS.Domain.Entities;

public class ExpertiseArea
{
    public int ExpertiseID { get; set; }
    public string ExpertiseName { get; set; } = string.Empty;
    public int? MajorID { get; set; }

    // Navigation
    public virtual Major? Major { get; set; }
    public virtual ICollection<LecturerExpertise> LecturerExpertises { get; set; } = new List<LecturerExpertise>();
}
