using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
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
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.Evaluations)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.MentorRoundReviews)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.Submissions)
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
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.Evaluations)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.MentorRoundReviews)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.Submissions)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Project>> GetDashboardProjectsAsync(int semesterId, int count) =>
        await _context.Projects
            .Where(p => p.SemesterID == semesterId)
            .Include(p => p.Major)
            .Include(p => p.Semester)
            .Include(p => p.ProjectSupervisors)
                .ThenInclude(ps => ps.Lecturer)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Project>> GetFilteredProjectsAsync(int? semesterId, string? status, string? search, string? majorName)
    {
        var query = _context.Projects.AsQueryable();

        // --- Filters applied in SQL ---
        if (semesterId.HasValue)
            query = query.Where(p => p.SemesterID == semesterId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProjectStatus>(status, out var parsedStatus))
            query = query.Where(p => p.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(majorName))
            query = query.Where(p => p.Major != null && p.Major.MajorName == majorName);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.ProjectCode.Contains(search) ||
                p.ProjectName.Contains(search) ||
                p.ProjectSupervisors.Any(ps => ps.Lecturer != null && ps.Lecturer.FullName.Contains(search)));

        // --- Lightweight Includes (no Evaluations/Submissions/MentorReviews) ---
        query = query
            .Include(p => p.Major)
            .Include(p => p.Semester)
            .Include(p => p.ProjectSupervisors)
                .ThenInclude(ps => ps.Lecturer)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.GroupMembers);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(int total, int withGroup, int missingSupervisor, int missingMembers, int draftCount, int activeCount, int completedCount)> GetDashboardStatsBySemesterAsync(int semesterId)
    {
        var total = await _context.Projects.CountAsync(p => p.SemesterID == semesterId);
        var withGroup = await _context.Projects.CountAsync(p => p.SemesterID == semesterId && p.ProjectGroups.Any());
        var missingSupervisor = await _context.Projects.CountAsync(p => p.SemesterID == semesterId && !p.ProjectSupervisors.Any());
        var missingMembers = await _context.Projects.CountAsync(p => p.SemesterID == semesterId && p.ProjectGroups.Any(g => !g.GroupMembers.Any()));
        
        var draftCount = await _context.Projects.CountAsync(p => p.SemesterID == semesterId && p.Status == ProjectStatus.Draft);
        var activeCount = await _context.Projects.CountAsync(p => p.SemesterID == semesterId && p.Status == ProjectStatus.Active);
        var completedCount = await _context.Projects.CountAsync(p => p.SemesterID == semesterId && p.Status == ProjectStatus.Completed);

        return (total, withGroup, missingSupervisor, missingMembers, draftCount, activeCount, completedCount);
    }

    public async Task<IEnumerable<Project>> GetSupervisorAssignmentProjectsAsync(int? semesterId)
    {
        var query = _context.Projects.AsQueryable();

        if (semesterId.HasValue)
            query = query.Where(p => p.SemesterID == semesterId.Value);

        // Explicitly exclude cancelled projects from assignment view
        query = query.Where(p => p.Status != ProjectStatus.Cancelled);

        return await query
            .Include(p => p.Major)
            .Include(p => p.Semester)
            .Include(p => p.ProjectGroups)
            .Include(p => p.ProjectSupervisors)
                .ThenInclude(ps => ps.Lecturer)
            .OrderByDescending(p => p.CreatedAt)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProgressProjectsAsync(int semesterId)
    {
        return await _context.Projects
            .Where(p => p.SemesterID == semesterId)
            .Include(p => p.Major)
            .Include(p => p.ProjectSupervisors)
                .ThenInclude(ps => ps.Lecturer)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.Submissions)
                    .ThenInclude(s => s.Requirement)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.MentorRoundReviews)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.Evaluations)
            .OrderByDescending(p => p.CreatedAt)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

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
                .ThenInclude(g => g.Evaluations)
            .Include(p => p.ProjectGroups)
                .ThenInclude(g => g.MentorRoundReviews)
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
                    .ThenInclude(m => m.User)
            .Where(p => p.ProjectGroups.Any(g => g.GroupMembers.Any(m => m.UserID == studentId)))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task AddAsync(Project project) =>
        await _context.Projects.AddAsync(project);

    public Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Project project)
    {
        _context.Projects.Remove(project);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
