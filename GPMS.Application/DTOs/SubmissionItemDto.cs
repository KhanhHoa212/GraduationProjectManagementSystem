using System;
using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs;

public class SubmissionItemDto
{
    public int RequirementID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    
    // Submission details (nullable if not yet submitted)
    public int? SubmissionID { get; set; }
    public string? FileName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public SubmissionStatus? Status { get; set; }
    
    // Helper to determine what to show in UI
    public bool IsSubmitted => SubmissionID.HasValue;
    public bool IsLate => !IsSubmitted && DateTime.UtcNow > Deadline;
}
