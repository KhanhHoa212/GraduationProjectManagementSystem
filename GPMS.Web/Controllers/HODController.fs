namespace GPMS.Web.Controllers

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization

[<Authorize(Roles = "HeadOfDept,Admin")>]
type HODController() =
    inherit Controller()

    member this.Index() = this.View()
    member this.Projects() = this.View()
    member this.ProjectDetails(id: string) = this.View()
    member this.Groups() = this.View()
    member this.GroupDetails(id: string) = this.View()
    member this.AssignSupervisor() = this.View()
    member this.Import() = this.View()
