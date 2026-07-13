using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class ReportTemplate
{
    public ulong Id { get; set; }

    public int? CreatedBy { get; set; }

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Options { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsShared { get; set; }
}
