using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface ISemesterRepository
{
    Task<Semester?> GetByIdAsync(int semesterId);
    Task<Semester?> GetByCodeAsync(string semesterCode);
    Task<IEnumerable<Semester>> GetAllAsync();
    Task AddAsync(Semester semester);
    Task UpdateAsync(Semester semester);
    Task DeleteAsync(int semesterId);
    Task<bool> ExistsAsync(int semesterId);
    Task<IEnumerable<ReviewRound>> GetRoundsBySemesterAsync(int semesterId);
    Task SaveChangesAsync();
}
