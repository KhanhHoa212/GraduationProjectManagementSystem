using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class Room
{
    public int RoomID { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string? Building { get; set; }
    public int Capacity { get; set; }
    public RoomType RoomType { get; set; } = RoomType.Classroom;
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public string? Notes { get; set; }

    // Navigation
    public virtual ICollection<ReviewSessionInfo> ReviewSessions { get; set; } = new List<ReviewSessionInfo>();
}
