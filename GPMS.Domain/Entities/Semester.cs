using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class Semester
{
    public int SemesterID { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SemesterStatus Status { get; set; } = SemesterStatus.Upcoming;

    // Navigation
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    public virtual ICollection<ReviewRound> ReviewRounds { get; set; } = new List<ReviewRound>();
}
