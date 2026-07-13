using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class UsersGroup
{
    public uint UserId { get; set; }

    public uint GroupId { get; set; }

    public ulong? CreatedBy { get; set; }
}
