using GPMS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace GPMS.Web.ViewModels;

public class SemesterViewModel
{
    public int SemesterID { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(4);
    public SemesterStatus Status { get; set; } = SemesterStatus.Upcoming;
    public int ProjectsCount { get; set; }
}

public class EditSemesterViewModel
{
    public int SemesterID { get; set; }

    [Required(ErrorMessage = "Semester code is required")]
    [StringLength(10, ErrorMessage = "Code is too long")]
    public string SemesterCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Academic year is required")]
    public string AcademicYear { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(4);

    public SemesterStatus Status { get; set; } = SemesterStatus.Upcoming;
    public string? Description { get; set; }
}
