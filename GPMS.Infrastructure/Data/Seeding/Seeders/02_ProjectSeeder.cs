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
        if (await _context.Projects.CountAsync() > 20) return;

        var faker = new Faker("vi");
        var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.Status == SemesterStatus.Active);
        var semesters = await _context.Semesters.Where(s => s.SemesterID >= 5).ToListAsync(); 
        
        var lecturers = await _context.Users
            .Where(u => u.UserID.StartsWith("GV"))
            .Select(u => u.UserID)
            .ToListAsync();
            
        if (!lecturers.Any()) return;

        var projects = new List<Project>();
        int projectCount = 0;

        foreach (var sem in semesters)
        {
            int count = sem.SemesterID == activeSemester?.SemesterID ? 80 : 20;

            for (int i = 0; i < count; i++)
            {
                projectCount++;
                var majorId = faker.PickRandom(1, 2); 
                var majorStr = majorId == 1 ? "SE" : "SS";
                
                ProjectStatus status;
                if (sem.Status == SemesterStatus.Closed) 
                    status = faker.Random.Double() > 0.1 ? ProjectStatus.Completed : ProjectStatus.Cancelled;
                else if (sem.Status == SemesterStatus.Active)
                    status = faker.Random.Double() > 0.2 ? ProjectStatus.Active : ProjectStatus.Draft;
                else
                    status = ProjectStatus.Draft;

                projects.Add(new Project
                {
                    ProjectCode = $"{sem.SemesterCode}{majorStr}_{projectCount:D3}",
                    ProjectName = faker.Company.CatchPhrase() + " " + faker.Hacker.Noun(),
                    Description = faker.Lorem.Paragraphs(2),
                    SemesterID = sem.SemesterID,
                    MajorID = majorId,
                    Status = status,
                    CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 60))
                });
            }
        }

        await _context.Projects.AddRangeAsync(projects);
        await _context.SaveChangesAsync();

        var projectSupervisors = new List<ProjectSupervisor>();
        foreach (var p in projects)
        {
            var pickedLecturers = faker.PickRandom(lecturers, faker.Random.Int(1, 2)).ToList();
            
            projectSupervisors.Add(new ProjectSupervisor
            {
                ProjectID = p.ProjectID,
                LecturerID = pickedLecturers[0],
                Role = ProjectRole.Main,
                AssignedAt = p.CreatedAt,
                AssignedBy = "ADMIN001"
            });

            if (pickedLecturers.Count > 1)
            {
                projectSupervisors.Add(new ProjectSupervisor
                {
                    ProjectID = p.ProjectID,
                    LecturerID = pickedLecturers[1],
                    Role = ProjectRole.Co,
                    AssignedAt = p.CreatedAt,
                    AssignedBy = "ADMIN001"
                });
            }
        }

        await _context.ProjectSupervisors.AddRangeAsync(projectSupervisors);
        await _context.SaveChangesAsync();
    }
}

