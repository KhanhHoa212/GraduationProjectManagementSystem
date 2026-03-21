using GPMS.Domain.Entities;

namespace GPMS.Application.Interfaces.Repositories;

public interface IMentorRoundReviewRepository
{
    Task<MentorRoundReview?> GetAsync(int reviewRoundId, int groupId);
    Task<IEnumerable<MentorRoundReview>> GetBySupervisorAsync(string supervisorId);
    Task AddAsync(MentorRoundReview review);
    void Update(MentorRoundReview review);
    Task SaveChangesAsync();
}
