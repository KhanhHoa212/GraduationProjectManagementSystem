using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GPMS.Domain.Entities;

namespace GPMS.Web.ViewModels;

public class MajorViewModel
{
    public int MajorID { get; set; }
    public string MajorCode { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public int FacultyID { get; set; }
}

public class EditMajorViewModel
{
    public int MajorID { get; set; }

    [Required]
    [Display(Name = "Major Code")]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "Major Code must be between 2 and 3 characters.")]
    public string MajorCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Major Name")]
    [StringLength(100, ErrorMessage = "Major Name cannot exceed 100 characters.")]
    public string MajorName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Faculty")]
    public int FacultyID { get; set; }

    public IEnumerable<Faculty>? Faculties { get; set; }
}
