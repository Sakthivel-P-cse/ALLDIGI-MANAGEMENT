using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class MaintenanceType
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public ulong? CreatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
