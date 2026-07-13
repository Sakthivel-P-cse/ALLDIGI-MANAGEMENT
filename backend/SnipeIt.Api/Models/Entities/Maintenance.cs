using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class Maintenance
{
    public uint Id { get; set; }

    public uint AssetId { get; set; }

    public int? SupplierId { get; set; }

    public string AssetMaintenanceType { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Url { get; set; }

    public bool IsWarranty { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public int? AssetMaintenanceTime { get; set; }

    public string? Notes { get; set; }

    public string? Image { get; set; }

    public decimal? Cost { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public ulong? MaintenanceTypeId { get; set; }

    public ulong? CheckedOutToId { get; set; }

    public string? CheckedOutToType { get; set; }

    public ulong? ResponsiblePartyId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public ulong? CompletedBy { get; set; }
}
