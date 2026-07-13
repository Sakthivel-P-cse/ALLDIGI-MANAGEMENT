namespace SnipeIt.Api.Dtos;

/// <summary>Response DTO for a department record.</summary>
public sealed class DepartmentResponseDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Populated by service.</summary>
    public int UsersCount { get; set; }
}

/// <summary>Payload for creating a department.</summary>
public sealed class CreateDepartmentDto
{
    public required string Name { get; set; }
    public int? CompanyId { get; set; }
    public int? LocationId { get; set; }
    public int? ManagerId { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Payload for updating a department. Null members are ignored.</summary>
public sealed class UpdateDepartmentDto
{
    public string? Name { get; set; }
    public int? CompanyId { get; set; }
    public int? LocationId { get; set; }
    public int? ManagerId { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}
