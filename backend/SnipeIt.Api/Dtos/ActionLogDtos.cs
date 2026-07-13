namespace SnipeIt.Api.Dtos;

public sealed class ActionLogResponseDto
{
    public uint Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public int? TargetId { get; set; }
    public string? TargetType { get; set; }
    public int? CreatedBy { get; set; }
    public string? Note { get; set; }
    public string? LogMeta { get; set; }
    public DateTime ActionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Display names
    public string? AdminName { get; set; }
    public string? TargetName { get; set; }
}
