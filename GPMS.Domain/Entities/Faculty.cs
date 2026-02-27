namespace GPMS.Domain.Entities;

public class Faculty
{
    public int FacultyID { get; set; }
    public string FacultyCode { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;

    // Navigation
    public virtual ICollection<Major> Majors { get; set; } = new List<Major>();
}
