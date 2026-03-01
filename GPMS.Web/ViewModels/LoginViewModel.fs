namespace GPMS.Web.ViewModels

open System.ComponentModel.DataAnnotations

type LoginViewModel() =
    [<Required>]
    member val Identifier = "" with get, set

    [<Required>]
    [<DataType(DataType.Password)>]
    member val Password = "" with get, set
