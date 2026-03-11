using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs;

public class ReviewRoundDto
{
    public int ReviewRoundID { get; set; }
    public int SemesterID { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public RoundType RoundType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmissionDeadline { get; set; }
    public string? Description { get; set; }
    public RoundStatus Status { get; set; }
    public List<SubmissionRequirementDto> SubmissionRequirements { get; set; } = new();
}

public class SubmissionRequirementDto
{
    public int RequirementID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AllowedFormats { get; set; }
    public int? MaxFileSizeMB { get; set; } = 50;
    public bool IsRequired { get; set; } = true;
    public DateTime Deadline { get; set; }
}

public class CreateReviewRoundDto
{
    public int SemesterID { get; set; }
    public int RoundNumber { get; set; }
    public RoundType RoundType { get; set; } = RoundType.Online;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmissionDeadline { get; set; }
    public string? Description { get; set; }
    public RoundStatus Status { get; set; } = RoundStatus.Planned;
    public List<SubmissionRequirementDto> SubmissionRequirements { get; set; } = new();
}
