using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GPMS.Infrastructure.Data.Seeding.Seeders;

public class UserSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public int Order => 1;

    public UserSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.CountAsync() >= 9 + 8) return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        // 2 New Lecturers
        var newLecturers = new List<User>
        {
            new User
            {
                UserID = "GV004",
                FullName = "Trần Thị Bích",
                Email = "bichtt@fpt.edu.vn",
                Username = "bichtt",
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UserCredentials = new List<UserCredential>
                {
                    new UserCredential { PasswordHash = passwordHash, AuthProvider = AuthProvider.Internal }
                }
            },
            new User
            {
                UserID = "GV005",
                FullName = "Lê Minh Khoa",
                Email = "khoalm@fpt.edu.vn",
                Username = "khoalm",
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UserCredentials = new List<UserCredential>
                {
                    new UserCredential { PasswordHash = passwordHash, AuthProvider = AuthProvider.Internal }
                }
            }
        };

        // 6 New Students
        var studentNames = new[] { "Nguyễn Văn An", "Trần Thị Bình", "Lê Hoàng Cường", "Phạm Minh Đức", "Hoàng Thu Thảo", "Đỗ Tuấn Anh" };
        var newStudents = new List<User>();
        for (int i = 0; i < 6; i++)
        {
            var studentId = $"SE18000{6 + i}";
            var names = studentNames[i].Split(' ');
            var lastName = names[^1].ToLower();
            newStudents.Add(new User
            {
                UserID = studentId,
                FullName = studentNames[i],
                Email = $"{lastName}{studentId.ToLower()}@fpt.edu.vn",
                Username = studentId.ToLower(),
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UserCredentials = new List<UserCredential>
                {
                    new UserCredential { PasswordHash = passwordHash, AuthProvider = AuthProvider.Internal }
                }
            });
        }

        foreach (var user in newLecturers)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == user.UserID))
            {
                _context.Users.Add(user);
                _context.UserRoles.Add(new UserRole { UserID = user.UserID, RoleName = RoleName.Lecturer, AssignedAt = DateTime.UtcNow });
            }
        }

        foreach (var user in newStudents)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == user.UserID))
            {
                _context.Users.Add(user);
                _context.UserRoles.Add(new UserRole { UserID = user.UserID, RoleName = RoleName.Student, AssignedAt = DateTime.UtcNow });
            }
        }

        await _context.SaveChangesAsync();
    }
}
