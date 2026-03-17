using GPMS.Application.DTOs;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface ISemesterService
{
    Task<IEnumerable<SemesterDto>> GetAllSemestersAsync(string? search = null, GPMS.Domain.Enums.SemesterStatus? status = null);
    Task<SemesterDto?> GetSemesterByIdAsync(int id);
    Task<string?> CreateSemesterAsync(CreateSemesterDto dto);
    Task<string?> UpdateSemesterAsync(UpdateSemesterDto dto);
    Task<bool> DeleteSemesterAsync(int id);
    Task<Semester?> GetOverlapSemesterAsync(DateTime start, DateTime end, int? ignoreId = null);
    Task<SemesterDto?> GetCurrentSemesterAsync();
}
