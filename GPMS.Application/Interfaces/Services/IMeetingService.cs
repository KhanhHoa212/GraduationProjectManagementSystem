using System;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IMeetingService
{
    Task<string> CreateOnlineMeetingAsync(string summary, string description, DateTime startTime, DateTime endTime);
}
