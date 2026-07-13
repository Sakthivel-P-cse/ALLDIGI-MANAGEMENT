using System;

namespace SnipeIt.Api.Models.Entities;

public partial class UserPreference
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public string Theme { get; set; } = "system";
    public string Language { get; set; } = "en";
    public string Timezone { get; set; } = "UTC";

    public virtual User User { get; set; } = null!;
}
