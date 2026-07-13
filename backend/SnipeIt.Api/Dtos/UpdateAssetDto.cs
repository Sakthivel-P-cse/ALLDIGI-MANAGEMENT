namespace SnipeIt.Api.Dtos;

/// <summary>
/// DTO for updating an existing asset
/// </summary>
public sealed class UpdateAssetDto
{
    /// <summary>Gets or sets the asset ID</summary>
    public uint Id { get; set; }

    /// <summary>Gets or sets the asset name</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the asset tag (unique identifier)</summary>
    public string? AssetTag { get; set; }

    /// <summary>Gets or sets the asset model ID</summary>
    public int? ModelId { get; set; }

    /// <summary>Gets or sets the serial number</summary>
    public string? Serial { get; set; }

    /// <summary>Gets or sets the purchase date</summary>
    public DateOnly? PurchaseDate { get; set; }

    /// <summary>Gets or sets the end-of-life date</summary>
    public DateOnly? AssetEolDate { get; set; }

    /// <summary>Gets or sets whether EOL is explicitly set</summary>
    public bool? EolExplicit { get; set; }

    /// <summary>Gets or sets the purchase cost</summary>
    public decimal? PurchaseCost { get; set; }

    /// <summary>Gets or sets the order number</summary>
    public string? OrderNumber { get; set; }

    /// <summary>Gets or sets the user ID this asset is assigned to</summary>
    public int? AssignedTo { get; set; }

    /// <summary>Gets or sets notes about the asset</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the status label ID</summary>
    public int? StatusId { get; set; }

    /// <summary>Gets or sets the warranty in months</summary>
    public int? WarrantyMonths { get; set; }

    /// <summary>Gets or sets whether the asset should be depreciated</summary>
    public bool? Depreciate { get; set; }

    /// <summary>Gets or sets the supplier ID</summary>
    public int? SupplierId { get; set; }

    /// <summary>Gets or sets whether the asset is requestable</summary>
    public bool? Requestable { get; set; }

    /// <summary>Gets or sets the ready-to-deploy location ID</summary>
    public int? RtdLocationId { get; set; }

    /// <summary>Gets or sets the location ID</summary>
    public int? LocationId { get; set; }

    /// <summary>Gets or sets the company ID</summary>
    public uint? CompanyId { get; set; }

    /// <summary>Gets or sets custom fields</summary>
    public Dictionary<string, string> CustomFields { get; set; } = new();

    /// <summary>Gets or sets the asset image (base64 or URL)</summary>
    public string? Image { get; set; }
}
