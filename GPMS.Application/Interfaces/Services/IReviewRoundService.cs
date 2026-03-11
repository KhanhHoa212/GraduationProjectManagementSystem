using GPMS.Application.DTOs;

namespace GPMS.Application.Interfaces.Services;

public interface IReviewRoundService
{
    Task<IEnumerable<ReviewRoundDto>> GetReviewRoundsBySemesterAsync(int semesterId);
    Task<ReviewRoundDto?> GetReviewRoundByIdAsync(int id);
    Task CreateReviewRoundAsync(CreateReviewRoundDto dto);
    Task UpdateReviewRoundAsync(int id, CreateReviewRoundDto dto);
    Task DeleteReviewRoundAsync(int id);
}
