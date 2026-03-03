namespace GPMS.Web.Controllers

open System
open System.Linq
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open GPMS.Application.Interfaces.Repositories
open GPMS.Domain.Entities
open GPMS.Domain.Enums
open GPMS.Web.ViewModels

[<Authorize(Roles = "Admin")>]
type SemesterController(semesterRepository: ISemesterRepository) =
    inherit Controller()

    member this.Index() =
        task {
            let! semesters = semesterRepository.GetAllAsync()
            let viewModels = 
                semesters 
                |> Seq.map (fun s -> 
                    let vm = SemesterViewModel()
                    vm.SemesterID <- s.SemesterID
                    vm.SemesterCode <- s.SemesterCode
                    vm.AcademicYear <- s.AcademicYear
                    vm.StartDate <- s.StartDate
                    vm.EndDate <- s.EndDate
                    vm.Status <- s.Status
                    vm.ProjectsCount <- (if s.Projects <> null then s.Projects.Count else 0)
                    vm)
                |> Seq.toList
            return this.View(viewModels) :> IActionResult
        }

    [<HttpGet>]
    member this.Create() =
        this.View(EditSemesterViewModel()) :> IActionResult

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Create(model: EditSemesterViewModel) =
        task {
            if not this.ModelState.IsValid then
                return this.View(model) :> IActionResult
            else
                let semester = Semester()
                semester.SemesterCode <- model.SemesterCode
                semester.AcademicYear <- model.AcademicYear
                semester.StartDate <- model.StartDate
                semester.EndDate <- model.EndDate
                semester.Status <- model.Status
                
                do! semesterRepository.AddAsync(semester)
                do! semesterRepository.SaveChangesAsync()
                this.TempData.["SuccessMessage"] <- "Created semester successfully!"
                return this.RedirectToAction("Index") :> IActionResult
        }

    [<HttpGet>]
    member this.Edit(id: int) =
        task {
            let! semester = semesterRepository.GetByIdAsync(id)
            if semester = null then return this.NotFound() :> IActionResult
            else
                let model = EditSemesterViewModel()
                model.SemesterID <- semester.SemesterID
                model.SemesterCode <- semester.SemesterCode
                model.AcademicYear <- semester.AcademicYear
                model.StartDate <- semester.StartDate
                model.EndDate <- semester.EndDate
                model.Status <- semester.Status
                return this.View(model) :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Edit(model: EditSemesterViewModel) =
        task {
            if not this.ModelState.IsValid then
                return this.View(model) :> IActionResult
            else
                let! semester = semesterRepository.GetByIdAsync(model.SemesterID)
                if semester = null then return this.NotFound() :> IActionResult
                else
                    semester.SemesterCode <- model.SemesterCode
                    semester.AcademicYear <- model.AcademicYear
                    semester.StartDate <- model.StartDate
                    semester.EndDate <- model.EndDate
                    semester.Status <- model.Status
                    
                    do! semesterRepository.UpdateAsync(semester)
                    do! semesterRepository.SaveChangesAsync()
                    this.TempData.["SuccessMessage"] <- "Updated semester successfully!"
                    return this.RedirectToAction("Index") :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Delete(id: int) =
        task {
            let! semester = semesterRepository.GetByIdAsync(id)
            if semester = null then return this.NotFound() :> IActionResult
            else
                let hasProjects = if semester.Projects <> null then semester.Projects.Count > 0 else false
                if hasProjects then
                    this.TempData.["ErrorMessage"] <- "Cannot delete semester because it has projects."
                else
                    do! semesterRepository.DeleteAsync(id)
                    do! semesterRepository.SaveChangesAsync()
                    this.TempData.["SuccessMessage"] <- "Deleted semester successfully!"
                return this.RedirectToAction("Index") :> IActionResult
        }
