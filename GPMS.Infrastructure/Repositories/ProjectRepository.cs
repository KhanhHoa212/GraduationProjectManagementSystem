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

    public async Task<Project?> GetByIdAsync(int projectId) =>
        await _context.Projects.FindAsync(projectId);

    public async Task<IEnumerable<Project>> GetAllAsync() =>
        await _context.Projects.ToListAsync();

    public async Task<IEnumerable<Project>> GetBySemesterAsync(int semesterId) =>
        await _context.Projects.Where(p => p.SemesterID == semesterId).ToListAsync();

    public async Task<IEnumerable<Project>> GetAllWithDetailsAsync() =>
        await _context.Projects
            .Include(p => p.Major)
            .Include(p => p.Semester)
            .Include(p => p.ProjectSupervisors)
                .ThenInclude(ps => ps.Lecturer)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.GroupMembers)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Project>> GetBySemesterWithDetailsAsync(int semesterId) =>
        await _context.Projects
            .Where(p => p.SemesterID == semesterId)
            .Include(p => p.Major)
            .Include(p => p.Semester)
            .Include(p => p.ProjectSupervisors)
                .ThenInclude(ps => ps.Lecturer)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.GroupMembers)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Project?> GetDetailAsync(int projectId) =>
        await _context.Projects
            .Include(p => p.Major)
            .Include(p => p.Semester)
            .Include(p => p.ProjectSupervisors)
                .ThenInclude(ps => ps.Lecturer)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.Submissions)
                    .ThenInclude(s => s.Requirement)
            .FirstOrDefaultAsync(p => p.ProjectID == projectId);

    public async Task<Project?> GetProjectByStudentIdAsync(string studentId) =>
        await _context.Projects
            .Include(p => p.Major)
            .Include(p => p.Semester)
            .Include(p => p.ProjectSupervisors)
                .ThenInclude(ps => ps.Lecturer)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.GroupMembers)
            .Where(p => p.ProjectGroups.Any(g => g.GroupMembers.Any(m => m.UserID == studentId)))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task AddAsync(Project project) =>
        await _context.Projects.AddAsync(project);

    public async Task UpdateAsync(Project project) =>
        _context.Projects.Update(project);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
