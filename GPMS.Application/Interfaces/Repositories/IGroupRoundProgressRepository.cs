using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IGroupRoundProgressRepository
{
    Task<IEnumerable<GroupRoundProgress>> GetByReviewRoundIdsAndGroupIdsAsync(IEnumerable<int> roundIds, IEnumerable<int> groupIds);
}
