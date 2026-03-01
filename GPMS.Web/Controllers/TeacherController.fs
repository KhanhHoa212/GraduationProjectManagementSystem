namespace GPMS.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization

[<Authorize(Roles = "Lecturer")>]
type TeacherController() =
    inherit Controller()

    member this.Index() =
        this.View()
