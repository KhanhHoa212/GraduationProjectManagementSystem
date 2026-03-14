using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs.Lecturer;

public class LecturerEvaluationFormViewModel
{
    public int EvaluationId { get; set; }
    public int AssignmentId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ReviewRoundName { get; set; } = string.Empty;
    
    public string SupervisorName { get; set; } = string.Empty;
    public DateTime DefenseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public List<StudentMemberInfo> Members { get; set; } = new();
    public List<ProjectDocument> Documents { get; set; } = new();
    
    public int ChecklistId { get; set; }
    public string ChecklistTitle { get; set; } = string.Empty;
    
    public List<EvaluationCriterion> Criteria { get; set; } = new();
    
    public Dictionary<int, decimal> Scores { get; set; } = new();
    public string OverallFeedback { get; set; } = string.Empty;
}

public class EvaluationCriterion
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public decimal? AwardedScore { get; set; }
    public string? Comment { get; set; }
}

public class ProjectDocument
{
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? Url { get; set; }
}
