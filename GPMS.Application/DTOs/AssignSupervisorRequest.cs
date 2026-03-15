namespace GPMS.Application.DTOs;

public class AssignSupervisorRequest
{
    public int ProjectID { get; set; }
    public string LecturerID { get; set; } = string.Empty;
}
