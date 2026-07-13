using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class PermissionGroup
{
    public uint Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Permissions { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public string? Notes { get; set; }

    public string? ScimExternalid { get; set; }
}
