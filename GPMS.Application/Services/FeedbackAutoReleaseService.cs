using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;

namespace GPMS.Application.Services;

public class FeedbackAutoReleaseService : IFeedbackAutoReleaseService
{
    private readonly IFeedbackRepository _feedbackRepository;

    public FeedbackAutoReleaseService(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<int> AutoReleasePendingFeedbacksAsync(CancellationToken cancellationToken = default)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-7);
        var pendingReleases = await _feedbackRepository
            .GetApprovalsPendingAutoReleaseAsync(thresholdDate, cancellationToken);

        if (pendingReleases.Count == 0)
        {
            return 0;
        }

        var autoReleasedAt = DateTime.UtcNow;
        foreach (var approval in pendingReleases)
        {
            approval.IsVisibleToStudent = true;
            approval.AutoReleasedAt = autoReleasedAt;
        }

        await _feedbackRepository.SaveChangesAsync();
        return pendingReleases.Count;
    }
}
