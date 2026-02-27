using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly GpmsDbContext _context;
    public ProjectRepository(GpmsDbContext context) => _context = context;

    public async Task<Project?> GetByIdAsync(int projectId) => await _context.Projects.FindAsync(projectId);
    public async Task<IEnumerable<Project>> GetAllAsync() => await _context.Projects.ToListAsync();
    public async Task<IEnumerable<Project>> GetBySemesterAsync(int semesterId) => 
        await _context.Projects.Where(p => p.SemesterID == semesterId).ToListAsync();
    public async Task AddAsync(Project project) => await _context.Projects.AddAsync(project);
    public async Task UpdateAsync(Project project) => _context.Projects.Update(project);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
