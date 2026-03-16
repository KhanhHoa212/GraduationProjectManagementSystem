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
        string userId = value.ToString()!.ToUpper();
        string role = model.Role;
        
        // Count leading letters
        int letterCount = 0;
        while (letterCount < userId.Length && char.IsLetter(userId[letterCount]))
        {
            letterCount++;
        }

        // Logic: 3+ characters only allowed for HeadOfDept
        if (role == "HeadOfDept")
        {
            if (letterCount >= 3)
            {
                // If it's a 3-character ID, we just allow it (or could add more rules for HOD format)
                return ValidationResult.Success;
            }
        }
        else
        {
            if (letterCount >= 3)
            {
                return new ValidationResult("chỉ HOD được sử dụng ID 3 kí tự");
            }
        }

        if (letterCount != 2)
        {
            return new ValidationResult("ID phải bắt đầu bằng 2 kí tự mã vùng và ngành");
        }

        char campus = userId[0];
        char dept = userId[1];

        var validCampuses = new[] { 'H', 'D', 'Q', 'C', 'S' };
        var validDepts = new[] { 'E', 'S', 'A' };

        if (!validCampuses.Contains(campus))
        {
            return new ValidationResult("kí tự đầu phải là H (Hà Nội), D (Đà Nẵng), Q (Quy Nhơn), C (Cần Thơ), S (TP.HCM)");
        }

        if (!validDepts.Contains(dept))
        {
            return new ValidationResult("kí tự thứ 2 phải là E (CNTT), S (Kinh tế), A (Ngôn ngữ)");
        }

        string remaining = userId.Substring(2);
        if (remaining.Length != 6 || !remaining.All(char.IsDigit))
        {
            return new ValidationResult("ID phải có 6 con số sau 2 kí tự đầu (VD: HE123456)");
        }

        return ValidationResult.Success;
    }
}
