namespace GPMS.Domain.Entities;

public class Major
{
    public int MajorID { get; set; }
    public string MajorCode { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public int FacultyID { get; set; }

    // Navigation
    public virtual Faculty Faculty { get; set; } = null!;
    public virtual ICollection<ExpertiseArea> ExpertiseAreas { get; set; } = new List<ExpertiseArea>();
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
