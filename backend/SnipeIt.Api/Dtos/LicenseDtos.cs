namespace SnipeIt.Api.Dtos;

/// <summary>License data returned to clients, including computed seat/expiry info.</summary>
public sealed class LicenseResponseDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? Serial { get; set; }
    public int Seats { get; set; }
    public int FreeSeats { get; set; }
    public int AssignedSeats { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string? OrderNumber { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public bool? Maintained { get; set; }
    public bool? Reassignable { get; set; }
    public int? SupplierId { get; set; }
    public int? ManufacturerId { get; set; }
    public int? CategoryId { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Computed: "active", "expiring" (within 30 days) or "expired".</summary>
    public string ExpiryStatus { get; set; } = "active";
}

/// <summary>A single license seat and its assignment.</summary>
public sealed class LicenseSeatDto
{
    public uint Id { get; set; }
    public int? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public int? AssetId { get; set; }
    public string? Notes { get; set; }
    public bool IsFree { get; set; }
}

public sealed class CreateLicenseDto
{
    public required string Name { get; set; }
    public string? Serial { get; set; }
    public int Seats { get; set; } = 1;
    public decimal? PurchaseCost { get; set; }
    public string? OrderNumber { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public bool? Maintained { get; set; }
    public bool? Reassignable { get; set; }
    public int? SupplierId { get; set; }
    public int? ManufacturerId { get; set; }
    public int? CategoryId { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateLicenseDto
{
    public string? Name { get; set; }
    public string? Serial { get; set; }
    public int? Seats { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string? OrderNumber { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public bool? Maintained { get; set; }
    public bool? Reassignable { get; set; }
    public int? SupplierId { get; set; }
    public int? ManufacturerId { get; set; }
    public int? CategoryId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Payload to assign a free seat to a user or asset.</summary>
public sealed class AssignSeatDto
{
    public int? AssignedTo { get; set; }
    public int? AssetId { get; set; }
    public string? Notes { get; set; }
}
