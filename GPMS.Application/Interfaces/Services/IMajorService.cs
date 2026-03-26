using GPMS.Application.DTOs;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IMajorService
{
    Task<IEnumerable<MajorDto>> GetAllMajorsAsync();
    Task<MajorDto?> GetMajorByIdAsync(int id);
    Task<string?> CreateMajorAsync(CreateMajorDto dto);
    Task<string?> UpdateMajorAsync(UpdateMajorDto dto);
    Task<bool> DeleteMajorAsync(int id);
    Task<IEnumerable<Faculty>> GetAllFacultiesAsync();
}
