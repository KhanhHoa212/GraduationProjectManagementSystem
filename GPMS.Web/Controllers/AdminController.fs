namespace GPMS.Web.Controllers

open System
open System.Linq
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open GPMS.Application.Interfaces.Repositories
open GPMS.Domain.Entities
open GPMS.Domain.Enums
open GPMS.Web.ViewModels

[<Authorize(Roles = "Admin,HeadOfDept")>]
type AdminController(userRepository: IUserRepository, semesterRepository: ISemesterRepository) =
    inherit Controller()

    [<Authorize(Roles = "Admin")>]
    member this.Dashboard() =
        this.View() :> IActionResult

    // --- USER MANAGEMENT ---

    member this.Index() =
        task {
            let! users = userRepository.GetAllAsync()
            let viewModels = 
                users 
                |> Seq.map (fun u -> 
                    let vm = UserViewModel()
                    vm.UserID <- u.UserID
                    vm.Username <- u.Username
                    vm.Email <- u.Email
                    vm.FullName <- u.FullName
                    vm.Phone <- u.Phone
                    vm.Status <- u.Status
                    vm.Roles.AddRange(u.UserRoles |> Seq.map (fun r -> r.RoleName.ToString()))
                    vm)
                |> Seq.toList
            return this.View(viewModels) :> IActionResult
        }

    [<HttpGet>]
    member this.Create() =
        this.View(EditUserViewModel()) :> IActionResult

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Create(model: EditUserViewModel) =
        task {
            if not this.ModelState.IsValid then
                return this.View(model) :> IActionResult
            else
                let newUser = User()
                newUser.UserID <- if String.IsNullOrWhiteSpace(model.UserID) then Guid.NewGuid().ToString().Substring(0, 20) else model.UserID
                newUser.Username <- model.Username
                newUser.Email <- model.Email
                newUser.FullName <- model.FullName
                newUser.Phone <- model.Phone
                newUser.Status <- model.Status
                
                if model.Role = "Admin" then
                    this.ModelState.AddModelError("Role", "Can not create Admin role.")
                    return this.View(model) :> IActionResult
                else
                    if model.Role = "Lecturer" then 
                        newUser.UserRoles.Add(UserRole(RoleName = RoleName.Lecturer, UserID = newUser.UserID))
                    else 
                        newUser.UserRoles.Add(UserRole(RoleName = RoleName.Student, UserID = newUser.UserID))

                    do! userRepository.AddAsync(newUser)
                    do! userRepository.SaveChangesAsync()
                    this.TempData.["SuccessMessage"] <- "Created user successfully!"
                    return this.RedirectToAction("Index") :> IActionResult
        }

    [<HttpGet>]
    member this.Edit(id: string) =
        task {
            let! user = userRepository.GetByIdAsync(id)
            if user = null then return this.NotFound() :> IActionResult
            else
                let model = EditUserViewModel()
                model.UserID <- user.UserID
                model.Username <- user.Username
                model.Email <- user.Email
                model.FullName <- user.FullName
                model.Phone <- user.Phone
                model.Status <- user.Status
                let role = 
                    if user.UserRoles |> Seq.exists (fun r -> r.RoleName = RoleName.Admin) then "Admin"
                    elif user.UserRoles |> Seq.exists (fun r -> r.RoleName = RoleName.Lecturer) then "Lecturer"
                    else "Student"
                model.Role <- role
                return this.View(model) :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Edit(model: EditUserViewModel) =
        task {
            if not this.ModelState.IsValid then
                return this.View(model) :> IActionResult
            else
                let! user = userRepository.GetByIdAsync(model.UserID)
                if user = null then return this.NotFound() :> IActionResult
                else
                    user.Username <- model.Username
                    user.Email <- model.Email
                    user.FullName <- model.FullName
                    user.Phone <- model.Phone
                    user.Status <- model.Status
                    
                    let wasAdmin = user.UserRoles |> Seq.exists (fun r -> r.RoleName = RoleName.Admin)
                    
                    if wasAdmin && model.Role <> "Admin" then
                        this.TempData.["ErrorMessage"] <- "Can not change Admin role."
                        return this.RedirectToAction("Index") :> IActionResult
                    else
                        user.UserRoles.Clear()
                        match model.Role with
                        | "Admin" -> user.UserRoles.Add(UserRole(RoleName = RoleName.Admin, UserID = user.UserID))
                        | "Lecturer" -> user.UserRoles.Add(UserRole(RoleName = RoleName.Lecturer, UserID = user.UserID))
                        | _ -> user.UserRoles.Add(UserRole(RoleName = RoleName.Student, UserID = user.UserID))

                        do! userRepository.UpdateAsync(user)
                        do! userRepository.SaveChangesAsync()
                        this.TempData.["SuccessMessage"] <- "Update user successfully!"
                        return this.RedirectToAction("Index") :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.ToggleStatus(id: string) =
        task {
            let! user = userRepository.GetByIdAsync(id)
            if user = null then return this.NotFound() :> IActionResult
            else
                let isAdmin = user.UserRoles |> Seq.exists (fun r -> r.RoleName = RoleName.Admin)
                if isAdmin then
                    this.TempData.["ErrorMessage"] <- "Can not change Admin status."
                    return this.RedirectToAction("Index") :> IActionResult
                else
                    user.Status <- if user.Status = UserStatus.Active then UserStatus.Inactive else UserStatus.Active
                    do! userRepository.UpdateAsync(user)
                    do! userRepository.SaveChangesAsync()
                    this.TempData.["SuccessMessage"] <- "Changed user status successfully!"
                    return this.RedirectToAction("Index") :> IActionResult
        }

    // --- SEMESTER MANAGEMENT ---

    [<Authorize(Roles = "Admin")>]
    member this.SemesterIndex() =
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
            return this.View("SemesterIndex", viewModels) :> IActionResult
        }

    [<Authorize(Roles = "Admin")>]
    [<HttpGet>]
    member this.SemesterCreate() =
        this.View("SemesterCreate", EditSemesterViewModel()) :> IActionResult

    [<Authorize(Roles = "Admin")>]
    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.SemesterCreate(model: EditSemesterViewModel) =
        task {
            if not this.ModelState.IsValid then
                return this.View("SemesterCreate", model) :> IActionResult
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
                return this.RedirectToAction("SemesterIndex") :> IActionResult
        }

    [<Authorize(Roles = "Admin")>]
    [<HttpGet>]
    member this.SemesterEdit(id: int) =
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
                return this.View("SemesterEdit", model) :> IActionResult
        }

    [<Authorize(Roles = "Admin")>]
    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.SemesterEdit(model: EditSemesterViewModel) =
        task {
            if not this.ModelState.IsValid then
                return this.View("SemesterEdit", model) :> IActionResult
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
                    return this.RedirectToAction("SemesterIndex") :> IActionResult
        }

    [<Authorize(Roles = "Admin")>]
    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.SemesterDelete(id: int) =
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
                return this.RedirectToAction("SemesterIndex") :> IActionResult
        }
