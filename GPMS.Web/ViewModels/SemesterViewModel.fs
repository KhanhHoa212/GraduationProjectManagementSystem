namespace GPMS.Web.ViewModels

open System
open System.ComponentModel.DataAnnotations
open GPMS.Domain.Enums

type SemesterViewModel() =
    member val SemesterID = 0 with get, set
    member val SemesterCode = "" with get, set
    member val AcademicYear = "" with get, set
    member val StartDate = DateTime.Now with get, set
    member val EndDate = DateTime.Now.AddMonths(4) with get, set
    member val Status = SemesterStatus.Upcoming with get, set
    member val ProjectsCount = 0 with get, set

type EditSemesterViewModel() =
    member val SemesterID = 0 with get, set
    
    [<Required(ErrorMessage = "Semester code is required")>]
    [<StringLength(10, ErrorMessage = "Code is too long")>]
    member val SemesterCode = "" with get, set
    
    [<Required(ErrorMessage = "Academic year is required")>]
    member val AcademicYear = "" with get, set
    
    [<Required>]
    member val StartDate = DateTime.Now with get, set
    
    [<Required>]
    member val EndDate = DateTime.Now.AddMonths(4) with get, set
    
    member val Status = SemesterStatus.Upcoming with get, set
    member val Description = "" with get, set
