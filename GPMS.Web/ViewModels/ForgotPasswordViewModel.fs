namespace GPMS.Web.ViewModels

open System.ComponentModel.DataAnnotations

type ForgotPasswordViewModel() =
    [<Required(ErrorMessage = "Email is required")>]
    [<EmailAddress(ErrorMessage = "Invalid email address")>]
    member val Email = "" with get, set

type ResetPasswordViewModel() =
    member val Token = "" with get, set
    
    [<Required(ErrorMessage = "Password is required")>]
    [<StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")>]
    member val NewPassword = "" with get, set

    [<Compare("NewPassword", ErrorMessage = "Passwords do not match")>]
    member val ConfirmPassword = "" with get, set
