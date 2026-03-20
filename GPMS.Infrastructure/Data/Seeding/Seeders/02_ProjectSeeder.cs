using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Data.Seeding.Seeders;

public class ProjectSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public int Order => 2;

    public ProjectSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.Status == SemesterStatus.Active);
        if (activeSemester == null) return;

        string semesterCode = activeSemester.SemesterCode;
        if (await _context.Projects.AnyAsync(p => p.ProjectCode.StartsWith(semesterCode + "SE"))) return;

        var projects = new List<Project>
        {
            new Project
            {
                ProjectCode = $"{semesterCode}SE01",
                ProjectName = "Hệ thống quản lý bán hàng đa kênh",
                Description = "Phát triển hệ thống quản lý bán hàng tích hợp nhiều sàn TMĐT.",
                SemesterID = activeSemester.SemesterID,
                MajorID = 1,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                ProjectCode = $"{semesterCode}SE02",
                ProjectName = "Ứng dụng học tiếng Anh qua video",
                Description = "App học tiếng Anh sử dụng trí tuệ nhân tạo để gợi ý nội dung.",
                SemesterID = activeSemester.SemesterID,
                MajorID = 1,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                ProjectCode = $"{semesterCode}SE03",
                ProjectName = "Platform quản lý sự kiện và đặt vé",
                Description = "Nền tảng cho phép tổ chức và quản lý bán vé sự kiện trực tuyến.",
                SemesterID = activeSemester.SemesterID,
                MajorID = 1,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                ProjectCode = $"{semesterCode}SE04",
                ProjectName = "Hệ thống theo dõi sức khỏe thông minh",
                Description = "IoT kết hợp Mobile App để theo dõi các chỉ số sức khỏe.",
                SemesterID = activeSemester.SemesterID,
                MajorID = 1,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                ProjectCode = $"{semesterCode}SE05",
                ProjectName = "Mạng xã hội cho cộng đồng lập trình viên",
                Description = "Nơi chia sẻ kiến thức và kết nối các dev với nhau.",
                SemesterID = activeSemester.SemesterID,
                MajorID = 1,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();

        // Assign Supervisors (GV001–GV005)
        var supervisors = new[] { "GV001", "GV002", "GV003", "GV004", "GV005" };
        for (int i = 0; i < projects.Count; i++)
        {
            _context.ProjectSupervisors.Add(new ProjectSupervisor
            {
                ProjectID = projects[i].ProjectID,
                LecturerID = supervisors[i % supervisors.Length],
                Role = ProjectRole.Main,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "ADMIN001"
            });
        }

        await _context.SaveChangesAsync();
    }
}
