using GPMS.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface ISemesterService
{
    Task<IEnumerable<SemesterDto>> GetAllSemestersAsync();
    Task<SemesterDto?> GetSemesterByIdAsync(int id);
    Task CreateSemesterAsync(CreateSemesterDto dto);
    Task UpdateSemesterAsync(UpdateSemesterDto dto);
    Task<bool> DeleteSemesterAsync(int id);
}
