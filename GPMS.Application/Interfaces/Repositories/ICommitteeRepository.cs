using System.Collections.Generic;
using System.Threading.Tasks;
using GPMS.Domain.Entities;

namespace GPMS.Application.Interfaces.Repositories;

public interface ICommitteeRepository
{
    Task<IEnumerable<Committee>> GetBySemesterAsync(int semesterId);
    Task<Committee?> GetByIdAsync(int id);
    Task AddAsync(Committee committee);
    Task UpdateAsync(Committee committee);
    Task DeleteAsync(Committee committee);
}
