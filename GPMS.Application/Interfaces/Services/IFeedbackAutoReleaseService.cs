using System.Threading;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IFeedbackAutoReleaseService
{
    Task<int> AutoReleasePendingFeedbacksAsync(CancellationToken cancellationToken = default);
}
