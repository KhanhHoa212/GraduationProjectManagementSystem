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
    [Required(ErrorMessage = "Student ID/User ID is required")]
    [UserIdValidation]
    public string UserID { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Phone number must be 10 or 11 digits.")]
    public string? Phone { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    [Required(ErrorMessage = "Please select a role")]
    public string Role { get; set; } = "Student";
}

public class UserIdValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var model = (EditUserViewModel)validationContext.ObjectInstance;
        var (isValid, errorMessage) = Helpers.ValidationHelper.ValidateUserId(value.ToString()!, model.Role);

        if (!isValid)
        {
            return new ValidationResult(errorMessage);
        }

        return ValidationResult.Success;
    }
}
