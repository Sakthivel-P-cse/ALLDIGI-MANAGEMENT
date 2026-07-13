using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class Manufacturer
{
    public uint Id { get; set; }

    public string Name { get; set; } = null!;

    public string? TagColor { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? Url { get; set; }

    public string? SupportUrl { get; set; }

    public string? WarrantyLookupUrl { get; set; }

    public string? SupportPhone { get; set; }

    public string? SupportEmail { get; set; }

    public string? Image { get; set; }

    public string? Notes { get; set; }
}
