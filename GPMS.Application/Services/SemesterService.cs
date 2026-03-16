using AutoMapper;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesterRepository;
    private readonly IMapper _mapper;

    public SemesterService(ISemesterRepository semesterRepository, IMapper mapper)
    {
        _semesterRepository = semesterRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SemesterDto>> GetAllSemestersAsync(string? search = null, SemesterStatus? status = null)
    {
        var semesters = await _semesterRepository.GetAllAsync();

        if (status.HasValue)
        {
            semesters = semesters.Where(s => s.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            semesters = semesters.Where(s => 
                s.SemesterCode.ToLower().Contains(search) || 
                s.AcademicYear.ToLower().Contains(search));
        }

        return _mapper.Map<IEnumerable<SemesterDto>>(semesters);
    }

    public async Task<SemesterDto?> GetSemesterByIdAsync(int id)
    {
        var s = await _semesterRepository.GetByIdAsync(id);
        if (s == null) return null;

        return _mapper.Map<SemesterDto>(s);
    }
    public async Task<string?> UpdateSemesterAsync(UpdateSemesterDto dto)
    {
        var s = await _semesterRepository.GetByIdAsync(dto.SemesterID);
        if (s == null)
            return "Semester not found.";

        if (dto.StartDate >= dto.EndDate)
            return "Start date must be before end date.";

        var months = (dto.EndDate.Year - dto.StartDate.Year) * 12 +
                     dto.EndDate.Month - dto.StartDate.Month;

        if (months < 3 || months > 4)
            return "Semester duration must be between 3 and 4 months.";

        var overlap = await _semesterRepository
            .GetOverlapSemesterAsync(dto.StartDate, dto.EndDate, dto.SemesterID);

        if (overlap != null)
            return $"Semester overlaps with existing semester {overlap.SemesterCode}";

        s.SemesterCode = dto.SemesterCode;
        s.AcademicYear = dto.AcademicYear;
        s.StartDate = dto.StartDate;
        s.EndDate = dto.EndDate;
        s.Status = dto.Status;

        await _semesterRepository.UpdateAsync(s);
        await _semesterRepository.SaveChangesAsync();

        return null;
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
    public async Task<string?> CreateSemesterAsync(CreateSemesterDto dto)
    {
        if (dto.StartDate >= dto.EndDate)
            return "Start date must be before end date.";

        var months = (dto.EndDate.Year - dto.StartDate.Year) * 12 +
                     dto.EndDate.Month - dto.StartDate.Month;

        if (months < 3 || months > 4)
            return "Semester duration must be between 3 and 4 months.";

        var overlap = await _semesterRepository
            .GetOverlapSemesterAsync(dto.StartDate, dto.EndDate);

        if (overlap != null)
            return $"Semester overlaps with existing semester {overlap.SemesterCode}";

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

        return null;
    }

    public Task<Semester?> GetOverlapSemesterAsync(DateTime start, DateTime end, int? ignoreId = null)
    {
        return _semesterRepository.GetOverlapSemesterAsync(start, end, ignoreId);
    }

    public async Task<SemesterDto?> GetCurrentSemesterAsync()
    {
        var semesters = await _semesterRepository.GetAllAsync();
        var now = DateTime.Now;
        // Prefer a semester whose date range includes today
        var current = semesters.FirstOrDefault(s => s.StartDate <= now && s.EndDate >= now);
        // Fallback: the most recently started semester
        current ??= semesters.OrderByDescending(s => s.StartDate).FirstOrDefault();
        return current == null ? null : _mapper.Map<SemesterDto>(current);
    }
}
