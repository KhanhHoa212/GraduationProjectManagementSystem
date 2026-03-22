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

    public async Task<IEnumerable<Major>> GetAllAsync() =>
        await _context.Majors.ToListAsync();

    public async Task<Major?> GetByNameAsync(string name) =>
        await _context.Majors.FirstOrDefaultAsync(m => m.MajorName == name);
}
