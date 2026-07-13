using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class KitsAccessory
{
    public uint Id { get; set; }

    public int? KitId { get; set; }

    public int? AccessoryId { get; set; }

    public int Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ulong? CreatedBy { get; set; }
}
