using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesterRepository;

    public SemesterService(ISemesterRepository semesterRepository)
    {
        _semesterRepository = semesterRepository;
    }

    public async Task<IEnumerable<SemesterDto>> GetAllSemestersAsync()
    {
        var semesters = await _semesterRepository.GetAllAsync();
        return semesters.Select(s => new SemesterDto
        {
            SemesterID = s.SemesterID,
            SemesterCode = s.SemesterCode,
            AcademicYear = s.AcademicYear,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status,
            ProjectsCount = s.Projects?.Count ?? 0
        });
    }

    public async Task<SemesterDto?> GetSemesterByIdAsync(int id)
    {
        var s = await _semesterRepository.GetByIdAsync(id);
        if (s == null) return null;

        return new SemesterDto
        {
            SemesterID = s.SemesterID,
            SemesterCode = s.SemesterCode,
            AcademicYear = s.AcademicYear,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status,
            ProjectsCount = s.Projects?.Count ?? 0
        };
    }

    public async Task CreateSemesterAsync(CreateSemesterDto dto)
    {
        var semester = new Semester
        {
            SemesterCode = dto.SemesterCode,
            AcademicYear = dto.AcademicYear,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status
        };

        await _semesterRepository.AddAsync(semester);
        await _semesterRepository.SaveChangesAsync();
    }

    public async Task UpdateSemesterAsync(UpdateSemesterDto dto)
    {
        var s = await _semesterRepository.GetByIdAsync(dto.SemesterID);
        if (s == null) return;

        s.SemesterCode = dto.SemesterCode;
        s.AcademicYear = dto.AcademicYear;
        s.StartDate = dto.StartDate;
        s.EndDate = dto.EndDate;
        s.Status = dto.Status;

        await _semesterRepository.UpdateAsync(s);
        await _semesterRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteSemesterAsync(int id)
    {
        var s = await _semesterRepository.GetByIdAsync(id);
        if (s == null) return false;

        var hasProjects = s.Projects?.Any() ?? false;
        if (hasProjects) return false;

        await _semesterRepository.DeleteAsync(id);
        await _semesterRepository.SaveChangesAsync();
        return true;
    }
}
