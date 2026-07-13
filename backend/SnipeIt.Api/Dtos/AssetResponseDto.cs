namespace SnipeIt.Api.Dtos;

/// <summary>
/// DTO for asset response
/// </summary>
public sealed class AssetResponseDto
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
    public bool EolExplicit { get; set; }

    /// <summary>Gets or sets the purchase cost</summary>
    public decimal? PurchaseCost { get; set; }

    /// <summary>Gets or sets the order number</summary>
    public string? OrderNumber { get; set; }

    /// <summary>Gets the asset image</summary>
    public string? Image { get; init; }

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
    public bool Requestable { get; set; }

    /// <summary>Gets or sets the ready-to-deploy location ID</summary>
    public int? RtdLocationId { get; set; }

    /// <summary>Gets or sets the location ID</summary>
    public int? LocationId { get; set; }

    /// <summary>Gets or sets the company ID</summary>
    public uint? CompanyId { get; set; }

    /// <summary>Gets or sets custom fields</summary>
    public Dictionary<string, string> CustomFields { get; set; } = new();

    /// <summary>Gets or sets the acceptance status</summary>
    public string? Accepted { get; set; }

    /// <summary>Gets or sets the last checkout date</summary>
    public DateTime? LastCheckout { get; set; }

    /// <summary>Gets or sets the last check-in date</summary>
    public DateTime? LastCheckin { get; set; }

    /// <summary>Gets or sets the expected check-in date</summary>
    public DateOnly? ExpectedCheckin { get; set; }

    /// <summary>Gets or sets the assigned type</summary>
    public string? AssignedType { get; set; }

    /// <summary>Gets or sets the last audit date</summary>
    public DateTime? LastAuditDate { get; set; }

    /// <summary>Gets or sets the next audit date</summary>
    public DateOnly? NextAuditDate { get; set; }

    /// <summary>Gets or sets the check-in counter</summary>
    public int CheckinCounter { get; set; }

    /// <summary>Gets or sets when the asset was created</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>Gets or sets when the asset was last updated</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Gets or sets when the asset was deleted</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Gets or sets whether the asset is archived</summary>
    public bool? Archived { get; set; }

    /// <summary>Gets or sets the asset model display name</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets the asset location display name</summary>
    public string? LocationName { get; set; }

    /// <summary>Gets or sets the assigned user's display name</summary>
    public string? AssignedToName { get; set; }

    /// <summary>Gets or sets the supplier name</summary>
    public string? SupplierName { get; set; }
}
