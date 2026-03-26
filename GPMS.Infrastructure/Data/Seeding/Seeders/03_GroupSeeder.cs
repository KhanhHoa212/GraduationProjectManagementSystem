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

public class GroupSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public int Order => 3;

    public GroupSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.ProjectGroups.CountAsync() > 10) return;

        var faker = new Faker("vi");
        
        var students = await _context.Users
            .Where(u => u.UserID.StartsWith("SE19"))
            .OrderBy(u => u.UserID)
            .ToListAsync();
            
        var projects = await _context.Projects
            .Where(p => p.SemesterID >= 5) // Active/Upcoming semesters
            .OrderBy(p => p.ProjectID)
            .ToListAsync();

        if (!students.Any() || !projects.Any()) return;

        // Shuffle projects to give random unique assignments
        var projectQueue = new Queue<Project>(faker.Random.Shuffle(projects));

        var groupMembers = new List<GroupMember>();
        int studentIdx = 0;
        int groupCounter = 1;

        while (studentIdx < students.Count && projectQueue.Any())
        {
            var project = projectQueue.Dequeue();
            
            var group = new ProjectGroup
            {
                ProjectID = project.ProjectID,
                GroupName = $"Team {groupCounter:D2}",
                CreatedAt = project.CreatedAt.AddDays(faker.Random.Int(1, 5))
            };
            
            _context.ProjectGroups.Add(group);
            await _context.SaveChangesAsync(); 

            int groupSize = faker.PickRandom(4, 5);
            bool hasLeader = false;

            for (int i = 0; i < groupSize && studentIdx < students.Count; i++)
            {
                groupMembers.Add(new GroupMember
                {
                    GroupID = group.GroupID,
                    UserID = students[studentIdx].UserID,
                    RoleInGroup = !hasLeader ? GroupRole.Leader : GroupRole.Member,
                    JoinedAt = group.CreatedAt.AddDays(1)
                });
                hasLeader = true;
                studentIdx++;
            }
            groupCounter++;
        }


        await _context.GroupMembers.AddRangeAsync(groupMembers);
        await _context.SaveChangesAsync();
    }
}
