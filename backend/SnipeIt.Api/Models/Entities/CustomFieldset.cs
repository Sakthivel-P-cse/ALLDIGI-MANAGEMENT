using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class CustomFieldset
{
    public uint Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }
}
