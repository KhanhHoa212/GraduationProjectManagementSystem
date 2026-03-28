using System.Collections.Generic;
using System.Threading.Tasks;
using GPMS.Application.DTOs;

namespace GPMS.Application.Interfaces.Services;

public interface ICommitteeService
{
    Task<ManageCommitteeViewModel> GetManageCommitteeDataAsync();
    Task<bool> CreateCommitteeAsync(CreateCommitteeRequest request);
    Task<bool> DeleteCommitteeAsync(int id);
    Task<CommitteeDto> GetCommitteeByIdAsync(int id);
    Task<bool> UpdateCommitteeAsync(int id, CreateCommitteeRequest request);
}
