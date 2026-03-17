using System.Linq;

namespace GPMS.Web.Helpers;

public static class ValidationHelper
{
    public static (bool isValid, string? errorMessage) ValidateUserId(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return (false, "User ID is required");

        userId = userId.ToUpper();
        
        int letterCount = 0;
        while (letterCount < userId.Length && char.IsLetter(userId[letterCount]))
        {
            letterCount++;
        }

        if (role == "HeadOfDept")
        {
            if (letterCount >= 3) return (true, null);
        }
        else
        {
            if (letterCount >= 3) return (false, "chỉ HOD được sử dụng ID 3 kí tự");
        }

        if (letterCount != 2)
            return (false, "ID phải bắt đầu bằng 2 kí tự mã vùng và ngành");

        char campus = userId[0];
        char dept = userId[1];

        var validCampuses = new[] { 'H', 'D', 'Q', 'C', 'S' };
        var validDepts = new[] { 'E', 'S', 'A' };

        if (!validCampuses.Contains(campus))
            return (false, "kí tự đầu phải là H (Hà Nội), D (Đà Nẵng), Q (Quy Nhơn), C (Cần Thơ), S (TP.HCM)");

        if (!validDepts.Contains(dept))
            return (false, "kí tự thứ 2 phải là E (CNTT), S (Kinh tế), A (Ngôn ngữ)");

        string remaining = userId.Substring(2);
        if (remaining.Length != 6 || !remaining.All(char.IsDigit))
            return (false, "ID phải có 6 con số sau 2 kí tự đầu (VD: HE123456)");

        return (true, null);
    }
}
