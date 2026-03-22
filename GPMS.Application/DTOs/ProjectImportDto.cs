using System.Collections.Generic;
using System.Linq; // Force Refresh 2026-03-22

namespace GPMS.Application.DTOs;

public class ProjectImportRowDto
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string SupervisorEmail { get; set; } = string.Empty;
    public List<string> StudentEmails { get; set; } = new();
}

public class ProjectImportPreviewDto
{
    public int RowIndex { get; set; }
    public ProjectImportRowDto Data { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool IsValid => !Errors.Any();
}
