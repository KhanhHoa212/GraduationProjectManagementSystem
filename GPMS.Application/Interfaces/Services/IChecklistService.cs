using GPMS.Application.DTOs;

namespace GPMS.Application.Interfaces.Services;

public interface IChecklistService
{
    Task<ChecklistDto?> GetByRoundIdAsync(int roundId);
    Task<(bool Success, string Message)> SaveChecklistAsync(SaveChecklistDto dto);
    Task<(bool Success, string Message)> CopyChecklistAsync(int fromSemesterId, int toSemesterId, List<int> roundNumbers);
    Task<IEnumerable<ChecklistDto>> GetBySemesterIdAsync(int semesterId);
}
