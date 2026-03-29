using System.Collections.Generic;

namespace GPMS.Application.DTOs;

public class CommitteeDto
{
    public int CommitteeID { get; set; }
    public string CommitteeName { get; set; } = string.Empty;
    public string ChairpersonName { get; set; } = string.Empty;
    public string SecretaryName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public string? Reviewer2Name { get; set; }
    public string? Reviewer3Name { get; set; }
    public string? ChairpersonID { get; set; }
    public string? SecretaryID { get; set; }
    public string? ReviewerID { get; set; }
    public string? Reviewer2ID { get; set; }
    public string? Reviewer3ID { get; set; }
}

public class ManageCommitteeViewModel
{
    public List<CommitteeDto> Committees { get; set; } = new();
    public List<UserDto> Lecturers { get; set; } = new();
}

public class CreateCommitteeRequest
{
    public string CommitteeName { get; set; } = string.Empty;
    public string ChairpersonID { get; set; } = string.Empty;
    public string SecretaryID { get; set; } = string.Empty;
    public string ReviewerID { get; set; } = string.Empty;
    public string? Reviewer2ID { get; set; }
    public string? Reviewer3ID { get; set; }
}
