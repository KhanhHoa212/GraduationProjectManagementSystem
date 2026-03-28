using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Domain.Entities;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Repositories;

public class CommitteeRepository : ICommitteeRepository
{
    private readonly GpmsDbContext _context;

    public CommitteeRepository(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Committee>> GetBySemesterAsync(int semesterId)
    {
        return await _context.Committees
            .Include(c => c.Chairperson)
            .Include(c => c.Secretary)
            .Include(c => c.Reviewer)
            .Where(c => c.SemesterID == semesterId)
            .ToListAsync();
    }

    public async Task<Committee?> GetByIdAsync(int id)
    {
        return await _context.Committees.FindAsync(id);
    }

    public async Task AddAsync(Committee committee)
    {
        await _context.Committees.AddAsync(committee);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Committee committee)
    {
        _context.Committees.Update(committee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Committee committee)
    {
        _context.Committees.Remove(committee);
        await _context.SaveChangesAsync();
    }
}
