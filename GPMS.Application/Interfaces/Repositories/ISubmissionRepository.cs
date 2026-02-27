using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface ISubmissionRepository
{
    Task<Submission?> GetByIdAsync(int submissionId);
    Task<IEnumerable<Submission>> GetByGroupAndRequirementAsync(int groupId, int requirementId);
    Task AddAsync(Submission submission);
    Task SaveChangesAsync();
}
