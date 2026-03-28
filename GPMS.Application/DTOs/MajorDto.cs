using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs;

public class MajorDto
{
    public int MajorID { get; set; }
    public string MajorCode { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public int FacultyID { get; set; }
    public string FacultyName { get; set; } = string.Empty;
}

public class CreateMajorDto
{
    public string MajorCode { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public int FacultyID { get; set; }
}

public class UpdateMajorDto
{
    public int MajorID { get; set; }
    public string MajorCode { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public int FacultyID { get; set; }
}
