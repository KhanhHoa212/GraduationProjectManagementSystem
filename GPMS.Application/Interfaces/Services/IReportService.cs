using GPMS.Application.DTOs;

namespace GPMS.Application.Interfaces.Services;

public interface IReportService
{
    Task<HODReportDto> GetHODReportAsync(int? semesterId);
}
