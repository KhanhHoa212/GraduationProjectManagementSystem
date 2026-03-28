namespace GPMS.Application.DTOs;

public class DashboardFeedbackDto
{
    public int FeedbackID { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // UI Helper properties
    public string TimeAgo => GetTimeAgo(CreatedAt);

    private static string GetTimeAgo(DateTime past)
    {
        var ts = DateTime.UtcNow - past;
        if (ts.TotalMinutes < 1) return "Just now";
        if (ts.TotalHours < 1) return $"{(int)ts.TotalMinutes}m ago";
        if (ts.TotalDays < 1) return $"{(int)ts.TotalHours}h ago";
        if (ts.TotalDays < 30) return $"{(int)ts.TotalDays}d ago";
        return past.ToString("MMM dd");
    }
}
