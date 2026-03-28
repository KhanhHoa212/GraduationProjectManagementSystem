namespace GPMS.Application.DTOs;

public class CreateDefenseSessionRequest
{
    public int ReviewRoundID { get; set; }
    public int GroupID { get; set; }
    public string ScheduledDate { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public int? RoomID { get; set; }
    public string MeetLink { get; set; }
    public string Notes { get; set; }
    public int? CommitteeID { get; set; }
    public bool IsOnline { get; set; }
}
