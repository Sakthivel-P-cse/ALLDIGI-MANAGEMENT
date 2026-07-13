namespace SnipeIt.Api.Dtos;

/// <summary>Response DTO for a location record.</summary>
public sealed class LocationResponseDto
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
    public string? Currency { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public uint? CompanyId { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Populated by service.</summary>
    public int AssetsCount { get; set; }
    public int UsersCount { get; set; }
    public int ChildrenCount { get; set; }
}

/// <summary>Payload for creating a location.</summary>
public sealed class CreateLocationDto
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
    public string? Currency { get; set; }
    public int? ParentId { get; set; }
    public int? ManagerId { get; set; }
    public uint? CompanyId { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Payload for updating a location. Null members are ignored.</summary>
public sealed class UpdateLocationDto
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
    public string? Currency { get; set; }
    public int? ParentId { get; set; }
    public int? ManagerId { get; set; }
    public uint? CompanyId { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}
