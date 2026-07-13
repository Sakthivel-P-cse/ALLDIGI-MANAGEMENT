namespace SnipeIt.Api.Dtos;

/// <summary>Response DTO for a model record.</summary>
public sealed class ModelResponseDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? ModelNumber { get; set; }
    public int? ManufacturerId { get; set; }
    public string? ManufacturerName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? DepreciationId { get; set; }
    public int? FieldsetId { get; set; }
    public int? Eol { get; set; }
    public bool RequireSerial { get; set; }
    public sbyte Requestable { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Populated by service.</summary>
    public int AssetsCount { get; set; }
}

/// <summary>Payload for creating a model.</summary>
public sealed class CreateModelDto
{
    public required string Name { get; set; }
    public string? ModelNumber { get; set; }
    public int? ManufacturerId { get; set; }
    public int? CategoryId { get; set; }
    public int? DepreciationId { get; set; }
    public int? FieldsetId { get; set; }
    public int? Eol { get; set; }
    public bool RequireSerial { get; set; }
    public sbyte Requestable { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Payload for updating a model. Null members are ignored.</summary>
public sealed class UpdateModelDto
{
    public string? Name { get; set; }
    public string? ModelNumber { get; set; }
    public int? ManufacturerId { get; set; }
    public int? CategoryId { get; set; }
    public int? DepreciationId { get; set; }
    public int? FieldsetId { get; set; }
    public int? Eol { get; set; }
    public bool? RequireSerial { get; set; }
    public sbyte? Requestable { get; set; }
    public string? Image { get; set; }
    public string? Notes { get; set; }
}
