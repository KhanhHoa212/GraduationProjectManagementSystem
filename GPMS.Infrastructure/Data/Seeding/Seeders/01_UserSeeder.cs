using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Bogus;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
        if (await _context.Users.CountAsync() > 20) 
        {
            Console.WriteLine("[UserSeeder] Users already exist (>20), skipping...");
            return;
        }
        Console.WriteLine("[UserSeeder] Seeding 30 Lecturers and 200 Students...");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        // Setting Bogus locale to Vietnamese
        var faker = new Faker("vi");

        // 1. Generate 30 Lecturers
        var newLecturers = new List<User>();
        for (int i = 1; i <= 30; i++)
        {
            var id = $"GV9{i:D3}";
            var fullName = faker.Name.FullName();
            var username = id.ToLower();
            
            newLecturers.Add(new User
            {
                UserID = id,
                FullName = fullName,
                Email = $"{username}@fpt.edu.vn",
                Username = username,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UserCredentials = new List<UserCredential>
                {
                    new UserCredential { PasswordHash = passwordHash, AuthProvider = AuthProvider.Internal }
                }
            });
        }

        // 2. Generate 200 Students
        var newStudents = new List<User>();
        for (int i = 1; i <= 200; i++)
        {
            var id = $"SE19{i:D4}";
            var fullName = faker.Name.FullName();
            var username = id.ToLower();

            newStudents.Add(new User
            {
                UserID = id,
                FullName = fullName,
                Email = $"{username}@fpt.edu.vn",
                Username = username,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UserCredentials = new List<UserCredential>
                {
                    new UserCredential { PasswordHash = passwordHash, AuthProvider = AuthProvider.Internal }
                }
            });
        }

        await _context.Users.AddRangeAsync(newLecturers);
        await _context.Users.AddRangeAsync(newStudents);
        
        await _context.UserRoles.AddRangeAsync(newLecturers.Select(l => new UserRole { UserID = l.UserID, RoleName = RoleName.Lecturer, AssignedAt = DateTime.UtcNow }));
        await _context.UserRoles.AddRangeAsync(newStudents.Select(s => new UserRole { UserID = s.UserID, RoleName = RoleName.Student, AssignedAt = DateTime.UtcNow }));

        Console.WriteLine($"[UserSeeder] Saving {newLecturers.Count + newStudents.Count} users and their roles...");
        await _context.SaveChangesAsync();
        Console.WriteLine("[UserSeeder] Save successful.");
    }
}

