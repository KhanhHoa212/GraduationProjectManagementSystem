using System.Threading;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IReminderService
{
    Task<int> ProcessAllRemindersAsync(CancellationToken cancellationToken = default);
}
