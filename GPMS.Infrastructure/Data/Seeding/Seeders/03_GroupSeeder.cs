using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        if (await _context.ProjectGroups.AnyAsync(g => g.GroupName.StartsWith("Team "))) return;

        var projects = await _context.Projects.Where(p => p.ProjectCode.StartsWith("SP25SE")).ToListAsync();
        var students = await _context.Users
            .Where(u => u.UserID.StartsWith("SE18"))
            .OrderBy(u => u.UserID)
            .Take(30) // Take up to 30 students for 10 groups
            .ToListAsync();

        int studentIdx = 0;
        foreach (var project in projects)
        {
            for (int g = 1; g <= 2; g++)
            {
                var @group = new ProjectGroup
                {
                    ProjectID = project.ProjectID,
                    GroupName = $"Team {(char)('A' + (studentIdx / 3))}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProjectGroups.Add(@group);
                await _context.SaveChangesAsync();

                // Add 3 members: 1 Leader + 2 Members
                for (int m = 0; m < 3; m++)
                {
                    if (studentIdx < students.Count)
                    {
                        _context.GroupMembers.Add(new GroupMember
                        {
                            GroupID = group.GroupID,
                            UserID = students[studentIdx].UserID,
                            RoleInGroup = m == 0 ? GroupRole.Leader : GroupRole.Member,
                            JoinedAt = DateTime.UtcNow
                        });
                        studentIdx++;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}
