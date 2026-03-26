using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class GroupRoundProgressRepository : IGroupRoundProgressRepository
{
    private readonly GpmsDbContext _context;

    public GroupRoundProgressRepository(GpmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GroupRoundProgress>> GetByReviewRoundIdsAndGroupIdsAsync(IEnumerable<int> roundIds, IEnumerable<int> groupIds)
    {
        return await _context.GroupRoundProgresses
            .Where(g => roundIds.Contains(g.ReviewRoundID) && groupIds.Contains(g.GroupID))
            .ToListAsync();
    }
}
