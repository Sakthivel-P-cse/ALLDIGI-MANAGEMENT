namespace SnipeIt.Api.Dtos;

/// <summary>Response DTO for a company record.</summary>
public sealed class CompanyResponseDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Populated by service.</summary>
    public int AssetsCount { get; set; }
    public int UsersCount { get; set; }
}

/// <summary>Payload for creating a company.</summary>
public sealed class CreateCompanyDto
{
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Payload for updating a company. Null members are ignored.</summary>
public sealed class UpdateCompanyDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}
