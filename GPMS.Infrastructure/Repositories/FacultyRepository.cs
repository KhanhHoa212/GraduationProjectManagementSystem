using GPMS.Application.Interfaces.Repositories;
using GPMS.Domain.Entities;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class FacultyRepository : IFacultyRepository
{
    private readonly GpmsDbContext _context;
    public FacultyRepository(GpmsDbContext context) => _context = context;

    public async Task<IEnumerable<Faculty>> GetAllAsync() =>
        await _context.Faculties.ToListAsync();

    public async Task<Faculty?> GetByIdAsync(int id) =>
        await _context.Faculties.FindAsync(id);
}
