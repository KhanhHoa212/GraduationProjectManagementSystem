using AutoMapper;
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
    private readonly IMapper _mapper;

    public SemesterService(ISemesterRepository semesterRepository, IMapper mapper)
    {
        _semesterRepository = semesterRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SemesterDto>> GetAllSemestersAsync()
    {
        var semesters = await _semesterRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SemesterDto>>(semesters);
    }

    public async Task<SemesterDto?> GetSemesterByIdAsync(int id)
    {
        var s = await _semesterRepository.GetByIdAsync(id);
        if (s == null) return null;

        return _mapper.Map<SemesterDto>(s);
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
