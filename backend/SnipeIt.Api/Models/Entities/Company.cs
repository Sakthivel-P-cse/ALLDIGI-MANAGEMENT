using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class Company
{
    public uint Id { get; set; }

    public string Name { get; set; } = null!;

    public string? TagColor { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Image { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ulong? CreatedBy { get; set; }

    public string? Notes { get; set; }
}
