using GPMS.Application.DTOs.Lecturer;

namespace GPMS.Application.Interfaces.Services;

public interface ILecturerScheduleService
{
    IReadOnlyList<LecturerScheduleEntryDto> BuildEntries(
        IEnumerable<ProjectGroupSummaryDto> groups,
        IEnumerable<ReviewAssignmentItemDto> assignments,
        DateTime now);

    Task<IReadOnlyList<LecturerDeadlineDto>> BuildDeadlinesAsync(
        IEnumerable<ProjectGroupSummaryDto> groups,
        IEnumerable<PendingFeedbackItemDto> pendingFeedbacks,
        DateTime now);

    Task<LecturerScheduleDto> BuildScheduleAsync(
        IEnumerable<ProjectGroupSummaryDto> groups,
        IEnumerable<ReviewAssignmentItemDto> assignments,
        IEnumerable<PendingFeedbackItemDto> pendingFeedbacks,
        string? roleFilter,
        string? rangeFilter,
        int weekOffset,
        DateTime now);
}
