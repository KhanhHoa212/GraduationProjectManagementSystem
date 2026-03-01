namespace GPMS.Web.ViewModels

open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open GPMS.Domain.Enums

type UserViewModel() =
    member val UserID = "" with get, set
    member val Username = "" with get, set
    member val Email = "" with get, set
    member val FullName = "" with get, set
    member val Phone = "" with get, set
    member val Status = UserStatus.Active with get, set
    member val Roles : List<string> = List<string>() with get, set

type EditUserViewModel() =
    [<Required>]
    member val UserID = "" with get, set
    
    [<Required>]
    member val FullName = "" with get, set
    
    [<EmailAddress>]
    member val Email = "" with get, set
    
    member val Username = "" with get, set
    member val Phone = "" with get, set
    member val Status = UserStatus.Active with get, set
    
    [<Required(ErrorMessage = "Please select a role")>]
    member val Role = "Student" with get, set
