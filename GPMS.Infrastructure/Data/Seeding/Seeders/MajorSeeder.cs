using GPMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Data.Seeding.Seeders;

public class MajorSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public MajorSeeder(GpmsDbContext context) => _context = context;

    public int Order => -1; // Run after faculty but before others

    public async Task SeedAsync()
    {
        var majors = new List<Major>
        {
            new Major { MajorID = 1, FacultyID = 1, MajorCode = "SE", MajorName = "Software Engineering" },
            new Major { MajorID = 2, FacultyID = 2, MajorCode = "DM", MajorName = "Digital Marketing" },
            new Major { MajorID = 3, FacultyID = 3, MajorCode = "GD", MajorName = "Graphic Design" },
            new Major { MajorID = 4, FacultyID = 4, MajorCode = "EN", MajorName = "English" },
            new Major { MajorID = 5, FacultyID = 4, MajorCode = "JP", MajorName = "Japanese" },
            new Major { MajorID = 6, FacultyID = 4, MajorCode = "KR", MajorName = "Korean" },
            new Major { MajorID = 7, FacultyID = 4, MajorCode = "CN", MajorName = "Chinese" },
            new Major { MajorID = 8, FacultyID = 1, MajorCode = "IA", MajorName = "Information Assurance" },
            new Major { MajorID = 9, FacultyID = 1, MajorCode = "IC", MajorName = "Integrated Circuit Design" },
            new Major { MajorID = 10, FacultyID = 2, MajorCode = "FI", MajorName = "Finance" },
            new Major { MajorID = 11, FacultyID = 2, MajorCode = "MK", MajorName = "Marketing" }
        };

        foreach (var m in majors)
        {
            var existing = await _context.Majors.FindAsync(m.MajorID);
            if (existing == null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Majors ON");
                    await _context.Majors.AddAsync(m);
                    await _context.SaveChangesAsync();
                    await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Majors OFF");
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            else
            {
                existing.FacultyID = m.FacultyID;
                existing.MajorCode = m.MajorCode;
                existing.MajorName = m.MajorName;
                _context.Majors.Update(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
