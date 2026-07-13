namespace SnipeIt.Api.Dtos;

/// <summary>Response DTO for a manufacturer record.</summary>
public sealed class ManufacturerResponseDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? SupportUrl { get; set; }
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
    public string? WarrantyLookupUrl { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Populated by service.</summary>
    public int AssetsCount { get; set; }
    public int LicensesCount { get; set; }
}

/// <summary>Payload for creating a manufacturer.</summary>
public sealed class CreateManufacturerDto
{
    public required string Name { get; set; }
    public string? Url { get; set; }
    public string? SupportUrl { get; set; }
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
    public string? WarrantyLookupUrl { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Payload for updating a manufacturer. Null members are ignored.</summary>
public sealed class UpdateManufacturerDto
{
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? SupportUrl { get; set; }
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
    public string? WarrantyLookupUrl { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}
