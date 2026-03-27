using GPMS.Application.DTOs;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IReviewRoundService
{
    Task<IEnumerable<ReviewRoundDto>> GetReviewRoundsBySemesterAsync(int semesterId);
    Task<ReviewRoundDto?> GetReviewRoundByIdAsync(int id);
    Task CreateReviewRoundAsync(CreateReviewRoundDto dto);
    Task UpdateReviewRoundAsync(int id, CreateReviewRoundDto dto);
    Task DeleteReviewRoundAsync(int id);
    Task<bool> InitializeDefaultRoundsAsync(int semesterId);
    
    // Session Management
    Task<IEnumerable<ReviewSessionInfo>> GetGroupSessionsAsync(int roundId);
    Task<bool> ScheduleSessionAsync(ScheduleSessionUpdateDto dto);
    Task<bool> GenerateMeetingLinkAsync(int sessionId);
}
