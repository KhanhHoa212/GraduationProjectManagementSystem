using GPMS.Domain.Enums;

namespace GPMS.Application.DTOs.Lecturer;

public class LecturerHistoryDto
{
    public List<LecturerReviewHistoryItemDto> ReviewHistory { get; set; } = new();
    public List<LecturerFeedbackHistoryItemDto> FeedbackHistory { get; set; } = new();
}

public class LecturerReviewHistoryItemDto
{
    public int EvaluationId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public string RoundType { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
    public DateTime SubmittedAt { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public string FeedbackPreview { get; set; } = string.Empty;
}

public class LecturerFeedbackHistoryItemDto
{
    public int FeedbackId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsVisibleToStudent { get; set; }
    public string? SupervisorComment { get; set; }
}
