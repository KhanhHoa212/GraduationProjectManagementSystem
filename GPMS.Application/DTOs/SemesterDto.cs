using GPMS.Domain.Enums;
using System;

namespace GPMS.Application.DTOs;

public class SemesterDto
{
    public int SemesterID { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SemesterStatus Status { get; set; }
    public int ProjectsCount { get; set; }
}

public class CreateSemesterDto
{
    public string SemesterCode { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SemesterStatus Status { get; set; }
}

public class UpdateSemesterDto
{
    public int SemesterID { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SemesterStatus Status { get; set; }
}
