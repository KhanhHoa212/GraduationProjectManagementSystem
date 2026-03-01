namespace GPMS.Web.Controllers

open System
open System.Security.Claims
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.Google
open GPMS.Application.Interfaces.Repositories
open GPMS.Domain.Entities

open GPMS.Domain.Enums
open GPMS.Web.ViewModels
open System.Web

type AuthController(userRepository: IUserRepository) =
    inherit Controller()

    let getRedirect (this: Controller) (user: User) =
        let roles = user.UserRoles |> Seq.map (fun ur -> ur.RoleName) |> Seq.toList
        if List.contains RoleName.Admin roles || List.contains RoleName.HeadOfDept roles then
            this.RedirectToAction("Index", "Admin") :> IActionResult
        elif List.contains RoleName.Lecturer roles then
            this.RedirectToAction("Index", "Teacher") :> IActionResult
        elif List.contains RoleName.Student roles then
            this.RedirectToAction("Index", "Student") :> IActionResult
        else
            this.RedirectToAction("Index", "Home") :> IActionResult

    let signInUser (this: Controller) (user: User) =
        task {
            let roleClaims = 
                user.UserRoles 
                |> Seq.map (fun ur -> Claim(ClaimTypes.Role, ur.RoleName.ToString()))
                |> Seq.toList

            let claims =
                [ Claim(ClaimTypes.NameIdentifier, user.UserID)
                  Claim(ClaimTypes.Email,          user.Email |> Option.ofObj |> Option.defaultValue "")
                  Claim(ClaimTypes.Name,           user.FullName) ]
                @ roleClaims

            let identity  = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
            let principal = ClaimsPrincipal(identity)
            let authProps = AuthenticationProperties(IsPersistent = true)

            do! this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps)
        }

    [<HttpGet>]
    member this.Login() =
        this.View()

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Login(model: LoginViewModel) =
        task {
            if not this.ModelState.IsValid then
                return this.View(model) :> IActionResult
            else
                let! user = userRepository.GetByUsernameOrEmailAsync(model.Identifier)
                
                let isValid =
                    match user with
                    | null -> false
                    | u ->
                        u.UserCredentials 
                        |> Seq.tryFind (fun c -> isNull c.ExternalProviderID && not (String.IsNullOrWhiteSpace(c.PasswordHash)))
                        |> Option.map (fun c -> BCrypt.Net.BCrypt.Verify(model.Password, c.PasswordHash))
                        |> Option.defaultValue false

                if isValid then
                    if user.Status = UserStatus.Inactive then
                        this.ModelState.AddModelError("", "Your account is deactivated. Please contact the administrator.")
                        return this.View(model) :> IActionResult
                    else
                        do! signInUser this user
                        return getRedirect this user
                else
                    this.ModelState.AddModelError("", "Invalid login attempt.")
                    return this.View(model) :> IActionResult
        }

    member this.GoogleLogin() =
        let redirectUrl = this.Url.Action("GoogleCallback", "Auth")
        let props = AuthenticationProperties(RedirectUri = redirectUrl)
        this.Challenge(props, GoogleDefaults.AuthenticationScheme)

    member this.GoogleCallback() =
        task {
            // 1. Read claims provided by Google
            let email    = this.User.FindFirst(ClaimTypes.Email)     |> Option.ofObj |> Option.map (fun c -> c.Value) |> Option.defaultValue ""
            let fullName = this.User.FindFirst(ClaimTypes.Name)      |> Option.ofObj |> Option.map (fun c -> c.Value) |> Option.defaultValue ""
            let picture  = this.User.FindFirst("urn:google:picture") |> Option.ofObj |> Option.map (fun c -> c.Value) |> Option.defaultValue null

            // 2. Look up the user in the DB, or create a new record
            let! existingUser = userRepository.GetByEmailAsync(email)

            let isAllowedDomain = email.EndsWith("@fe.edu.vn", StringComparison.OrdinalIgnoreCase) || email.EndsWith("@fpt.edu.vn", StringComparison.OrdinalIgnoreCase)
            if not isAllowedDomain && existingUser = null then
                this.TempData.["ErrorMessage"] <- "Only @fe.edu.vn and @fpt.edu.vn accounts or pre-registered personal Gmails are allowed to login."
                return this.RedirectToAction("Login") :> IActionResult
            else

            let! dbUser =
                task {
                    if existingUser = null then
                        // Auto-provision a new user from their Google profile
                        let newUser = User()
                        newUser.UserID    <- Guid.NewGuid().ToString().Substring(0, 20)
                        newUser.Email     <- email
                        newUser.FullName  <- fullName
                        newUser.AvatarUrl <- picture
                        do! userRepository.AddAsync(newUser)
                        do! userRepository.SaveChangesAsync()
                        return newUser
                    else
                        // Refresh display name and avatar on every login
                        existingUser.FullName  <- fullName
                        existingUser.AvatarUrl <- if picture <> null then picture else existingUser.AvatarUrl
                        do! userRepository.UpdateAsync(existingUser)
                        do! userRepository.SaveChangesAsync()
                        return existingUser
                }

            // 3. User status check
            if dbUser.Status = UserStatus.Inactive then
                this.TempData.["ErrorMessage"] <- "Your account is deactivated. Please contact the administrator."
                return this.RedirectToAction("Login") :> IActionResult
            else
                // 4. Sign in and redirect based on role
                do! signInUser this dbUser
                return getRedirect this dbUser
        }

    member this.Logout() =
        task {
            do! this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
            return this.RedirectToAction("Login") :> IActionResult
        }

    [<HttpGet>]
    member this.ForgotPassword() =
        this.View()

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.ForgotPassword(model: ForgotPasswordViewModel) =
        task {
            if not this.ModelState.IsValid then
                return this.View(model) :> IActionResult
            else
                let! user = userRepository.GetByEmailAsync(model.Email)
                if user <> null then
                    let credential = 
                        user.UserCredentials 
                        |> Seq.tryFind (fun c -> c.AuthProvider = AuthProvider.Internal || isNull c.ExternalProviderID)
                    match credential with
                    | Some c ->
                        let token = Guid.NewGuid().ToString("N")
                        c.PasswordResetToken <- token
                        c.PasswordResetExpiry <- Nullable(DateTime.UtcNow.AddHours(2.0))
                        do! userRepository.UpdateAsync(user)
                        do! userRepository.SaveChangesAsync()
                        
                        // Mock email sending
                        let resetLink = this.Url.Action("ResetPassword", "Auth", {| token = token |}, this.Request.Scheme)
                        printfn "PASSWORD RESET LINK: %s" resetLink
                    | None -> ()
                
                this.ViewData.["SuccessMessage"] <- "If the email exists in the system, we have sent a password reset link."
                return this.View() :> IActionResult
        }

    [<HttpGet>]
    member this.ResetPassword(token: string) =
        if String.IsNullOrWhiteSpace(token) then
            this.RedirectToAction("Login") :> IActionResult
        else
            let model = ResetPasswordViewModel(Token = token)
            this.View(model) :> IActionResult

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.ResetPassword(model: ResetPasswordViewModel) =
        task {
            if not this.ModelState.IsValid then
                return this.View(model) :> IActionResult
            else
                let! users = userRepository.GetAllAsync()
                let userWithToken = 
                    users 
                    |> Seq.tryFind (fun u -> 
                        u.UserCredentials 
                        |> Seq.exists (fun c -> c.PasswordResetToken = model.Token && c.PasswordResetExpiry.HasValue && c.PasswordResetExpiry.Value > DateTime.UtcNow))
                
                match userWithToken with
                | Some user ->
                    let credential = user.UserCredentials |> Seq.find (fun c -> c.PasswordResetToken = model.Token)
                    credential.PasswordHash <- BCrypt.Net.BCrypt.HashPassword(model.NewPassword)
                    credential.PasswordResetToken <- null
                    credential.PasswordResetExpiry <- Nullable()
                    do! userRepository.UpdateAsync(user)
                    do! userRepository.SaveChangesAsync()
                    this.TempData.["SuccessMessage"] <- "Password reset successfully! Please login!"
                    return this.RedirectToAction("Login") :> IActionResult
                | None ->
                    this.ModelState.AddModelError("", "Invalid or expired link.")
                    return this.View(model) :> IActionResult
        }