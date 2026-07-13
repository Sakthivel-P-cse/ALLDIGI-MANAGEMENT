using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class SamlNonce
{
    public ulong Id { get; set; }

    public string Nonce { get; set; } = null!;

    public DateTime NotValidAfter { get; set; }
}
