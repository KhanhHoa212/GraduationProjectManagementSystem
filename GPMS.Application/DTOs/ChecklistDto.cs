using System.ComponentModel.DataAnnotations;

namespace GPMS.Application.DTOs;

public class ChecklistDto
{
    public int ChecklistID { get; set; }
    public int ReviewRoundID { get; set; }
    public string ReviewRoundTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ChecklistItemDto> Items { get; set; } = new();
}

public class ChecklistItemDto
{
    public int ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemContent { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; }
    public int OrderIndex { get; set; }
}

public class SaveChecklistDto
{
    public int ReviewRoundID { get; set; }
    public string Title { get; set; } = "Review Checklist";
    public string? Description { get; set; }
    public List<ChecklistItemDto> Items { get; set; } = new();
}
