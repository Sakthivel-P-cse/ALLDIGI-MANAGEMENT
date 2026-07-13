using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class AccessoriesCheckout
{
    public uint Id { get; set; }

    public int? CreatedBy { get; set; }

    public int? AccessoryId { get; set; }

    public int? AssignedTo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Note { get; set; }

    public string? AssignedType { get; set; }
}
