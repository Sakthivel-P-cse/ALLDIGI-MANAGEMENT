using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class CompanyUser
{
    public ulong Id { get; set; }

    public uint CompanyId { get; set; }

    public uint UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
