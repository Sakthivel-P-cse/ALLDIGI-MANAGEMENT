namespace SnipeIt.Api.Dtos;

/// <summary>Response DTO for a supplier record.</summary>
public sealed class SupplierResponseDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Zip { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Url { get; set; }
    public string? Contact { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Populated by service.</summary>
    public int AssetsCount { get; set; }
    public int LicensesCount { get; set; }
    public int AccessoriesCount { get; set; }
}

/// <summary>Payload for creating a supplier.</summary>
public sealed class CreateSupplierDto
{
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Zip { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Url { get; set; }
    public string? Contact { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Payload for updating a supplier. Null members are ignored.</summary>
public sealed class UpdateSupplierDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Zip { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Url { get; set; }
    public string? Contact { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}
