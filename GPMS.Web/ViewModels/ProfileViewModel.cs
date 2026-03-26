using System.ComponentModel.DataAnnotations;

namespace GPMS.Web.ViewModels;

public class ProfileViewModel
{
    public string UserID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid Phone Number")]
    public string? Phone { get; set; }

    [Display(Name = "Avatar URL")]
    public string? AvatarUrl { get; set; }

    [Display(Name = "Upload New Avatar")]
    public Microsoft.AspNetCore.Http.IFormFile? AvatarFile { get; set; }

    public List<string> Roles { get; set; } = new();
}
