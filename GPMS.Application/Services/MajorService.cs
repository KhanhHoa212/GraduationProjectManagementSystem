using AutoMapper;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class MajorService : IMajorService
{
    private readonly IMajorRepository _majorRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IMapper _mapper;

    public MajorService(IMajorRepository majorRepository, IFacultyRepository facultyRepository, IMapper mapper)
    {
        _majorRepository = majorRepository;
        _facultyRepository = facultyRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MajorDto>> GetAllMajorsAsync()
    {
        var majors = await _majorRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<MajorDto>>(majors);
    }

    public async Task<MajorDto?> GetMajorByIdAsync(int id)
    {
        var major = await _majorRepository.GetByIdAsync(id);
        return major == null ? null : _mapper.Map<MajorDto>(major);
    }

    public async Task<string?> CreateMajorAsync(CreateMajorDto dto)
    {
        var existing = await _majorRepository.GetByNameAsync(dto.MajorName);
        if (existing != null) return "Major name already exists.";

        var major = _mapper.Map<Major>(dto);
        await _majorRepository.AddAsync(major);
        await _majorRepository.SaveChangesAsync();
        return null;
    }

    public async Task<string?> UpdateMajorAsync(UpdateMajorDto dto)
    {
        var major = await _majorRepository.GetByIdAsync(dto.MajorID);
        if (major == null) return "Major not found.";

        _mapper.Map(dto, major);
        await _majorRepository.UpdateAsync(major);
        await _majorRepository.SaveChangesAsync();
        return null;
    }

    public async Task<bool> DeleteMajorAsync(int id)
    {
        var major = await _majorRepository.GetByIdAsync(id);
        if (major == null) return false;

        // Check if there are projects associated with this major
        if (major.Projects != null && major.Projects.Count > 0)
            return false;

        await _majorRepository.DeleteAsync(id);
        await _majorRepository.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Faculty>> GetAllFacultiesAsync()
    {
        return await _facultyRepository.GetAllAsync();
    }
}
