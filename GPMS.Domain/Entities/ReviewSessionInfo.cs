namespace GPMS.Domain.Entities;

public class ReviewSessionInfo
{
    public int SessionID { get; set; }
    public int ReviewRoundID { get; set; }
    public int GroupID { get; set; }
    public string? MeetLink { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int? RoomID { get; set; }
    public string? Notes { get; set; }
    public int? CommitteeID { get; set; }
    public bool IsOnline { get; set; }
    public virtual Committee? Committee { get; set; }

    // Navigation
    public virtual ReviewRound ReviewRound { get; set; } = null!;
    public virtual ProjectGroup Group { get; set; } = null!;
    public virtual Room? Room { get; set; }
}
