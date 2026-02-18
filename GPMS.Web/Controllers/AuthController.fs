namespace GPMS.Web.Controllers

open Microsoft.AspNetCore.Mvc

type AuthController() =
    inherit Controller()

    member this.Login() =
        this.View()
