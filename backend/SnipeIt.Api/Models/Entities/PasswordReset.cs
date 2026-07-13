using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class PasswordReset
{
    public string Email { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public uint Id { get; set; }
}
