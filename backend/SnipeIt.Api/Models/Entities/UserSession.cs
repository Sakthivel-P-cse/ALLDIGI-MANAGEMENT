using System;

namespace SnipeIt.Api.Models.Entities;

public partial class UserSession
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public string DeviceName { get; set; } = null!;
    public string IpAddress { get; set; } = null!;
    public DateTime LastActive { get; set; }
    public bool IsRevoked { get; set; }

    public virtual User User { get; set; } = null!;
}
