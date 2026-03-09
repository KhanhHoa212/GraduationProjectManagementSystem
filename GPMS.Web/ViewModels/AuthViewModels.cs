using System.ComponentModel.DataAnnotations;

namespace GPMS.Web.ViewModels;

public class LoginViewModel
{
    [Required]
    public string Identifier { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    public string NewPassword { get; set; } = string.Empty;

    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
