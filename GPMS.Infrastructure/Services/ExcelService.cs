using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Services;
using GPMS.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using GPMS.Domain.Enums;
using GPMS.Domain.Entities;

namespace GPMS.Infrastructure.Services;

public class ExcelService : IExcelService
{
    private readonly GpmsDbContext _context;

    public ExcelService(GpmsDbContext context)
    {
        _context = context;
        ExcelPackage.License.SetNonCommercialOrganization("GPMS");
    }

    public async Task<byte[]> GenerateProjectImportTemplateAsync()
    {
        using var package = new ExcelPackage();
        
        // 1. Fetch Data
        var majors = await _context.Majors.Select(m => m.MajorName).ToListAsync();
        var mentors = await _context.Users
            .Include(u => u.UserRoles)
            .Where(u => u.UserRoles.Any(ur => ur.RoleName == RoleName.Lecturer))
            .OrderBy(u => u.FullName)
            .Select(u => u.Email)
            .ToListAsync();
            
        var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.Status == SemesterStatus.Active);
        if (activeSemester == null) return Array.Empty<byte>();
        
        var studentsInGroups = await _context.GroupMembers
            .Include(gm => gm.Group)
                .ThenInclude(g => g.Project)
            .Where(gm => gm.Group.Project.SemesterID == activeSemester.SemesterID)
            .Select(gm => gm.UserID)
            .ToListAsync();
            
        var eligibleStudents = await _context.Users
            .Include(u => u.UserRoles)
            .Where(u => u.UserRoles.Any(ur => ur.RoleName == RoleName.Student) && !studentsInGroups.Contains(u.UserID))
            .OrderBy(u => u.UserID)
            .Select(u => new { u.UserID, u.FullName, u.Email })
            .ToListAsync();

        // Style constants
        var fptOrange = System.Drawing.Color.FromArgb(243, 112, 33);
        var fptBlue = System.Drawing.Color.FromArgb(30, 58, 95);
        var tableHeaderGray = System.Drawing.Color.FromArgb(242, 242, 242);

        // --- SHEET 1: HƯỚNG DẪN (GUIDE) ---
        var guideSheet = package.Workbook.Worksheets.Add("HƯỚNG DẪN");
        guideSheet.TabColor = System.Drawing.Color.FromArgb(0, 112, 192);
        guideSheet.View.ShowGridLines = false;

        guideSheet.Cells["B2:F2"].Merge = true;
        guideSheet.Cells["B2"].Value = "HƯỚNG DẪN NHẬP DỮ LIỆU ĐỀ TÀI & NHÓM";
        guideSheet.Cells["B2"].Style.Font.Size = 18;
        guideSheet.Cells["B2"].Style.Font.Bold = true;
        guideSheet.Cells["B2"].Style.Font.Color.SetColor(fptBlue);

        guideSheet.Cells["B4"].Value = "1. Cột có dấu (*) ";
        guideSheet.Cells["C4"].Value = "Là bắt buộc không được bỏ trống.";
        guideSheet.Cells["B4"].Style.Font.Bold = true;

        guideSheet.Cells["B5"].Value = "2. Chuyên ngành & Mentor";
        guideSheet.Cells["C5"].Value = "Sử dụng danh sách thả xuống (Drop-down) để chọn giá trị chính xác.";
        guideSheet.Cells["B5"].Style.Font.Bold = true;

        guideSheet.Cells["B6"].Value = "3. MSSV";
        guideSheet.Cells["C6"].Value = "Mỗi đề tài có tối đa 5 sinh viên. Sinh viên đầu tiên (Cột E) sẽ được định danh là NHÓM TRƯỞNG.";
        guideSheet.Cells["B6"].Style.Font.Bold = true;

        guideSheet.Cells["B8"].Value = "QUY ĐỊNH MÀU SẮC:";
        guideSheet.Cells["B8"].Style.Font.Bold = true;

        guideSheet.Cells["B9"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        guideSheet.Cells["B9"].Style.Fill.BackgroundColor.SetColor(fptOrange);
        guideSheet.Cells["C9"].Value = "Thông tin bắt buộc";

        guideSheet.Cells["B10"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        guideSheet.Cells["B10"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(191, 191, 191));
        guideSheet.Cells["C10"].Value = "Thông tin không bắt buộc";

        guideSheet.Column(2).Width = 25;
        guideSheet.Column(3).Width = 80;

        // --- SHEET 2: NHẬP DỮ LIỆU (PROJECTS_IMPORT) ---
        var sheet = package.Workbook.Worksheets.Add("Projects_Import");
        sheet.TabColor = fptOrange;
        sheet.Cells.Style.Font.Name = "Segoe UI";
        sheet.Cells.Style.Font.Size = 10;

        // Intro Area at the top
        sheet.Cells["A1:J1"].Merge = true;
        sheet.Cells["A1"].Value = "BIỂU MẪU NHẬP LIỆU DỰ ÁN VÀ NHÓM";
        sheet.Cells["A1"].Style.Font.Size = 14;
        sheet.Cells["A1"].Style.Font.Bold = true;
        sheet.Cells["A1"].Style.Font.Color.SetColor(fptBlue);
        sheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        sheet.Cells["A2:J2"].Merge = true;
        sheet.Cells["A2"].Value = "Vui lòng điền đầy đủ thông tin vào các cột bắt buộc (*). Thứ tự cột: Mã -> Tên -> Mô tả -> Chuyên ngành -> Mentor -> Students.";
        sheet.Cells["A2"].Style.Font.Italic = true;
        sheet.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);
        sheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        string[] headers = { 
            "Mã Đề Tài (*)", 
            "Tên Đề Tài (*)", 
            "Mô Tả Đề Tài", 
            "Chuyên Ngành (*)", 
            "Email Mentor (*)", 
            "MSSV Nhóm Trưởng (*)", 
            "MSSV Thành Viên 2", 
            "MSSV Thành Viên 3", 
            "MSSV Thành Viên 4", 
            "MSSV Thành Viên 5" 
        };

        const int headerRow = 4;
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cells[headerRow, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            
            // Required columns (0,1,3,4,5) in Orange, optional in Gray
            var isRequired = i == 0 || i == 1 || i == 3 || i == 4 || i == 5;
            cell.Style.Fill.BackgroundColor.SetColor(isRequired ? fptOrange : System.Drawing.Color.FromArgb(165, 165, 165));
        }
        sheet.Row(headerRow).Height = 25;

        // Set column widths
        sheet.Column(1).Width = 15; // Code
        sheet.Column(2).Width = 45; // Name
        sheet.Column(3).Width = 45; // Description
        sheet.Column(4).Width = 30; // Major
        sheet.Column(5).Width = 35; // Mentor
        for (int i = 6; i <= 10; i++) sheet.Column(i).Width = 22; // Students

        // --- HIDDEN SHEET: REFERENCE DATA ---
        var refSheet = package.Workbook.Worksheets.Add("_ReferenceData");
        refSheet.Hidden = eWorkSheetHidden.VeryHidden;
        refSheet.Cells["A1"].Value = "Majors";
        for (int i = 0; i < majors.Count; i++) refSheet.Cells[i + 2, 1].Value = majors[i];
        refSheet.Cells["B1"].Value = "Mentors";
        for (int i = 0; i < mentors.Count; i++) refSheet.Cells[i + 2, 2].Value = mentors[i];

        // --- SHEET 3: DANH SÁCH SINH VIÊN (ELIGIBLE_STUDENTS) ---
        var studentSheet = package.Workbook.Worksheets.Add("Eligible_Students");
        studentSheet.TabColor = System.Drawing.Color.FromArgb(0, 176, 80);
        studentSheet.View.FreezePanes(3, 1);

        studentSheet.Cells["A1:C1"].Merge = true;
        studentSheet.Cells["A1"].Value = $"DANH SÁCH SINH VIÊN ĐỦ ĐIỀU KIỆN - HỌC KỲ: {activeSemester.SemesterCode}";
        studentSheet.Cells["A1"].Style.Font.Bold = true;
        studentSheet.Cells["A1"].Style.Font.Size = 14;
        studentSheet.Cells["A1"].Style.Font.Color.SetColor(fptBlue);

        studentSheet.Cells["A2"].Value = "MSSV";
        studentSheet.Cells["B2"].Value = "Họ Tên";
        studentSheet.Cells["C2"].Value = "Email";
        using (var range = studentSheet.Cells["A2:C2"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(tableHeaderGray);
            range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }

        for (int i = 0; i < eligibleStudents.Count; i++)
        {
            studentSheet.Cells[i + 3, 1].Value = eligibleStudents[i].UserID;
            studentSheet.Cells[i + 3, 2].Value = eligibleStudents[i].FullName;
            studentSheet.Cells[i + 3, 3].Value = eligibleStudents[i].Email;
        }
        studentSheet.Cells[studentSheet.Dimension.Address].AutoFitColumns();

        // --- ADD VALIDATIONS ---
        const string entryRange = "A5:J200";
        using (var r = sheet.Cells[entryRange])
        {
            r.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            r.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            r.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            r.Style.Border.Top.Color.SetColor(System.Drawing.Color.FromArgb(230, 230, 230));
            r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.FromArgb(230, 230, 230));
            r.Style.Border.Left.Color.SetColor(System.Drawing.Color.FromArgb(230, 230, 230));
            r.Style.Border.Right.Color.SetColor(System.Drawing.Color.FromArgb(230, 230, 230));
        }

        if (majors.Any())
        {
            var majorValidation = sheet.DataValidations.AddListValidation("D5:D200");
            majorValidation.Formula.ExcelFormula = $"_ReferenceData!$A$2:$A${majors.Count + 1}";
            majorValidation.ShowErrorMessage = true;
            majorValidation.ErrorTitle = "Sai chuyên ngành";
            majorValidation.Error = "Vui lòng chọn trong danh sách có sẵn!";
            majorValidation.PromptTitle = "Chọn chuyên ngành";
            majorValidation.Prompt = "Nhấp vào mũi tên để chọn chuyên ngành.";
        }
        
        if (mentors.Any())
        {
            var mentorValidation = sheet.DataValidations.AddListValidation("E5:E200");
            mentorValidation.Formula.ExcelFormula = $"_ReferenceData!$B$2:$B${mentors.Count + 1}";
            mentorValidation.ShowErrorMessage = true;
            mentorValidation.ErrorTitle = "Sai Mentor";
            mentorValidation.Error = "Vui lòng chọn Email của Mentor có trong hệ thống!";
            mentorValidation.PromptTitle = "Chọn Mentor";
            mentorValidation.Prompt = "Nhấp vào mũi tên để chọn Email người hướng dẫn.";
        }

        sheet.View.FreezePanes(5, 1);
        sheet.Select("A5");
        package.Workbook.Worksheets.MoveToStart("HƯỚNG DẪN");
        
        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<IEnumerable<ProjectImportPreviewDto>> PreviewProjectImportAsync(IFormFile file)
    {
        var result = new List<ProjectImportPreviewDto>();
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        
        var sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name == "Projects_Import") ?? package.Workbook.Worksheets[0];
        int rowCount = sheet.Dimension?.Rows ?? 0;
        
        // Basic data to validate against
        var majors = await _context.Majors.ToListAsync();
        var lecturers = await _context.Users
            .Include(u => u.UserRoles)
            .Where(u => u.UserRoles.Any(ur => ur.RoleName == RoleName.Lecturer))
            .ToListAsync();
        var students = await _context.Users
            .Include(u => u.UserRoles)
            .Where(u => u.UserRoles.Any(ur => ur.RoleName == RoleName.Student))
            .ToListAsync();
        
        var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.Status == SemesterStatus.Active);
        if (activeSemester == null) return result;
        
        var studentsWithGroups = await _context.GroupMembers
            .Include(gm => gm.Group)
                .ThenInclude(g => g.Project)
            .Where(gm => gm.Group.Project.SemesterID == activeSemester.SemesterID)
            .Select(gm => gm.UserID)
            .ToListAsync();

        var studentsInThisFile = new HashSet<string>();

        for (int row = 5; row <= rowCount; row++)
        {
            var projectCode = sheet.Cells[row, 1].Value?.ToString()?.Trim();
            var projectName = sheet.Cells[row, 2].Value?.ToString()?.Trim();
            var description = sheet.Cells[row, 3].Value?.ToString()?.Trim();
            var majorName = sheet.Cells[row, 4].Value?.ToString()?.Trim();
            var mentorEmail = sheet.Cells[row, 5].Value?.ToString()?.Trim();
            
            if (string.IsNullOrEmpty(projectName) && string.IsNullOrEmpty(projectCode)) continue;

            var dto = new ProjectImportPreviewDto 
            { 
                RowIndex = row,
                Data = new ProjectImportRowDto
                {
                    ProjectCode = projectCode ?? "",
                    ProjectName = projectName ?? "",
                    Description = description ?? "",
                    MajorName = majorName ?? "",
                    SupervisorEmail = mentorEmail ?? ""
                }
            };

            // Validation basics
            if (string.IsNullOrEmpty(projectName)) dto.Errors.Add("Tên đề tài không được để trống");
            if (string.IsNullOrEmpty(projectCode)) dto.Errors.Add("Mã đề tài không được để trống");
            
            if (string.IsNullOrEmpty(majorName) || !majors.Any(m => m.MajorName.Equals(majorName, StringComparison.OrdinalIgnoreCase)))
                dto.Errors.Add($"Chuyên ngành '{majorName}' không hợp lệ hoặc không tồn tại");
                
            if (string.IsNullOrEmpty(mentorEmail) || !lecturers.Any(l => l.Email?.Equals(mentorEmail, StringComparison.OrdinalIgnoreCase) == true))
                dto.Errors.Add($"Mentor '{mentorEmail}' không tồn tại trong hệ thống");

            // Students validation index starts at 6 (MSSV Nhóm Trưởng) to 10
            for (int col = 6; col <= 10; col++)
            {
                var mssv = sheet.Cells[row, col].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(mssv))
                {
                    if (dto.Data.StudentEmails.Contains(mssv))
                    {
                        dto.Errors.Add($"MSSV '{mssv}' bị trùng lặp trong cùng một nhóm (Dòng {row})");
                        continue;
                    }

                    if (studentsInThisFile.Contains(mssv))
                    {
                        dto.Errors.Add($"Sinh viên '{mssv}' đã xuất hiện ở một hàng khác trong file này. Mỗi sinh viên chỉ được tham gia 1 đề tài.");
                    }

                    dto.Data.StudentEmails.Add(mssv);
                    studentsInThisFile.Add(mssv);
                    var student = students.FirstOrDefault(s => s.UserID == mssv);
                    if (student == null)
                    {
                        dto.Errors.Add($"Sinh viên MSSV '{mssv}' không tồn tại");
                    }
                    else if (studentsWithGroups.Contains(mssv))
                    {
                        dto.Errors.Add($"Sinh viên '{mssv}' đã có nhóm trong học kỳ này");
                    }
                }
            }
            
            if (!dto.Data.StudentEmails.Any())
                dto.Errors.Add("Dự án phải có ít nhất 1 sinh viên (Nhóm trưởng)");

            result.Add(dto);
        }

        return result;
    }
}
