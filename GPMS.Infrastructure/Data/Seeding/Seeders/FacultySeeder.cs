using GPMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Data.Seeding.Seeders;

public class FacultySeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;
    public FacultySeeder(GpmsDbContext context) => _context = context;

    public int Order => -2; // Run first

    public async Task SeedAsync()
    {
        var faculties = new List<Faculty>
        {
            new Faculty { FacultyID = 1, FacultyCode = "IT", FacultyName = "Information Technology Faculty" },
            new Faculty { FacultyID = 2, FacultyCode = "EC", FacultyName = "Economy Faculty" },
            new Faculty { FacultyID = 3, FacultyCode = "DA", FacultyName = "Digital Arts Faculty" },
            new Faculty { FacultyID = 4, FacultyCode = "LA", FacultyName = "Language Faculty" }
        };

        foreach (var f in faculties)
        {
            var existing = await _context.Faculties.FindAsync(f.FacultyID);
            if (existing == null)
            {
                // To insert with explicit ID, we use SET IDENTITY_INSERT
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Faculties ON");
                    await _context.Faculties.AddAsync(f);
                    await _context.SaveChangesAsync();
                    await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Faculties OFF");
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
                existing.FacultyCode = f.FacultyCode;
                existing.FacultyName = f.FacultyName;
                _context.Faculties.Update(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
