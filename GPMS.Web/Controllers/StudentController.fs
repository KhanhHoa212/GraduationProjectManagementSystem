namespace GPMS.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization

[<Authorize(Roles = "Student")>]
type StudentController() =
    inherit Controller()

    member this.Index() =
        this.View()
