namespace SnipeIt.Api.Dtos;

/// <summary>Response DTO for a status label record.</summary>
public sealed class StatusLabelResponseDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }

    /// <summary>Computed type: "deployable" | "pending" | "archived" | "undeployable".</summary>
    public string StatusType { get; set; } = "undeployable";

    public bool Deployable { get; set; }
    public bool Pending { get; set; }
    public bool Archived { get; set; }
    public string? Color { get; set; }
    public bool? ShowInNav { get; set; }
    public bool? DefaultLabel { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Populated by service.</summary>
    public int AssetsCount { get; set; }
}

/// <summary>Payload for creating a status label.</summary>
public sealed class CreateStatusLabelDto
{
    public required string Name { get; set; }

    /// <summary>One of: "deployable", "pending", "archived", "undeployable".</summary>
    public required string StatusType { get; set; }

    public string? Color { get; set; }
    public bool? ShowInNav { get; set; }
    public bool? DefaultLabel { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Payload for updating a status label. Null members are ignored.</summary>
public sealed class UpdateStatusLabelDto
{
    public string? Name { get; set; }
    public string? StatusType { get; set; }
    public string? Color { get; set; }
    public bool? ShowInNav { get; set; }
    public bool? DefaultLabel { get; set; }
    public string? Notes { get; set; }
}
