using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class MajorRepository : IMajorRepository
{
    private readonly GpmsDbContext _context;
    public MajorRepository(GpmsDbContext context) => _context = context;

    public async Task<Major?> GetByIdAsync(int majorId) =>
        await _context.Majors.Include(m => m.Faculty).FirstOrDefaultAsync(m => m.MajorID == majorId);

    public async Task<IEnumerable<Major>> GetAllAsync() =>
        await _context.Majors.Include(m => m.Faculty).ToListAsync();

    public async Task<Major?> GetByNameAsync(string name) =>
        await _context.Majors.FirstOrDefaultAsync(m => m.MajorName == name);

    public async Task AddAsync(Major major) =>
        await _context.Majors.AddAsync(major);

    public Task UpdateAsync(Major major)
    {
        _context.Majors.Update(major);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int majorId)
    {
        var major = await _context.Majors.FindAsync(majorId);
        if (major != null) _context.Majors.Remove(major);
    }

    public async Task<bool> ExistsAsync(int majorId) =>
        await _context.Majors.AnyAsync(m => m.MajorID == majorId);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
