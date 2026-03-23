using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IMajorRepository
{
    Task<IEnumerable<Major>> GetAllAsync();
    Task<Major?> GetByNameAsync(string name);
}
