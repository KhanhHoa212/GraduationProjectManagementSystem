using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface IMajorRepository
{
    Task<Major?> GetByIdAsync(int majorId);
    Task<IEnumerable<Major>> GetAllAsync();
    Task<Major?> GetByNameAsync(string name);
    Task AddAsync(Major major);
    Task UpdateAsync(Major major);
    Task DeleteAsync(int majorId);
    Task<bool> ExistsAsync(int majorId);
    Task SaveChangesAsync();
}
