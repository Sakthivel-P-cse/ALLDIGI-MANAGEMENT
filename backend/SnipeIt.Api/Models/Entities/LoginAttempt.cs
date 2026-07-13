using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class LoginAttempt
{
    public uint Id { get; set; }

    public string? Username { get; set; }

    public string? RemoteIp { get; set; }

    public string? UserAgent { get; set; }

    public bool? Successful { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
