using GPMS.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GPMS.Web.ViewModels;

public class UserViewModel
{
    public string UserID { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public List<string> Roles { get; set; } = new();
}

public class EditUserViewModel
{
    [Required]
    public string UserID { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public string? Username { get; set; }
    public string? Phone { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    [Required(ErrorMessage = "Please select a role")]
    public string Role { get; set; } = "Student";
}
