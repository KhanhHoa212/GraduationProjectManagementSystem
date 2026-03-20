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
    public string? ItemName { get; set; }
    public string ItemType { get; set; } = "YesNo";
    public string? Section { get; set; }
    public int OrderIndex { get; set; }
    public List<RubricDescriptionDto> Rubrics { get; set; } = new();
}

public class RubricDescriptionDto
{
    public int RubricID { get; set; }
    public string GradeLevel { get; set; } = string.Empty; // Excellent, Good, Acceptable, Fail
    public string Description { get; set; } = string.Empty;
}

public class SaveChecklistDto
{
    public int ReviewRoundID { get; set; }
    public string Title { get; set; } = "Review Checklist";
    public string? Description { get; set; }
    public List<ChecklistItemDto> Items { get; set; } = new();
}

public class CopyChecklistRequest
{
    public int FromSemesterId { get; set; }
    public int ToSemesterId { get; set; }
    public List<int> RoundNumbers { get; set; } = new();
}
