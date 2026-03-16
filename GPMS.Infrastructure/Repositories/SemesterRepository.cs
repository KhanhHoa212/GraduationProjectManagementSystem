using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class SemesterRepository : ISemesterRepository
{
    private readonly GpmsDbContext _context;
    public SemesterRepository(GpmsDbContext context) => _context = context;

    public async Task<Semester?> GetByIdAsync(int semesterId) => 
        await _context.Semesters.Include(s => s.Projects).FirstOrDefaultAsync(s => s.SemesterID == semesterId);

    public async Task<Semester?> GetByCodeAsync(string semesterCode) => 
        await _context.Semesters.FirstOrDefaultAsync(s => s.SemesterCode == semesterCode);

    public async Task<IEnumerable<Semester>> GetAllAsync() => 
        await _context.Semesters.Include(s => s.Projects).ToListAsync();

    public async Task AddAsync(Semester semester) => await _context.Semesters.AddAsync(semester);
    public async Task UpdateAsync(Semester semester) => _context.Semesters.Update(semester);
    
    public async Task DeleteAsync(int semesterId)
    {
        var semester = await _context.Semesters.FindAsync(semesterId);
        if (semester != null) _context.Semesters.Remove(semester);
    }

    public async Task<bool> ExistsAsync(int semesterId) => await _context.Semesters.AnyAsync(s => s.SemesterID == semesterId);

    public async Task<IEnumerable<ReviewRound>> GetRoundsBySemesterAsync(int semesterId)
    {
        return await _context.ReviewRounds
            .Where(r => r.SemesterID == semesterId)
            .OrderBy(r => r.RoundNumber)
            .ToListAsync();
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
