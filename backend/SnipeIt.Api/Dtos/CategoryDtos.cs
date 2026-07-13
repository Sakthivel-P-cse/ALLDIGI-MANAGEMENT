namespace SnipeIt.Api.Dtos;

/// <summary>Response DTO for a category record.</summary>
public sealed class CategoryResponseDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? CategoryType { get; set; }
    public bool UseDefaultEula { get; set; }
    public bool RequireAcceptance { get; set; }
    public bool CheckinEmail { get; set; }
    public bool AlertOnResponse { get; set; }
    public string? TagColor { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Computed count of assets in this category (populated by service).</summary>
    public int AssetsCount { get; set; }
}

/// <summary>Payload for creating a new category.</summary>
public sealed class CreateCategoryDto
{
    public required string Name { get; set; }
    public string? CategoryType { get; set; }
    public bool UseDefaultEula { get; set; }
    public bool RequireAcceptance { get; set; }
    public bool CheckinEmail { get; set; }
    public bool AlertOnResponse { get; set; }
    public string? TagColor { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Payload for updating an existing category. Null members are ignored.</summary>
public sealed class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? CategoryType { get; set; }
    public bool? UseDefaultEula { get; set; }
    public bool? RequireAcceptance { get; set; }
    public bool? CheckinEmail { get; set; }
    public bool? AlertOnResponse { get; set; }
    public string? TagColor { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}
