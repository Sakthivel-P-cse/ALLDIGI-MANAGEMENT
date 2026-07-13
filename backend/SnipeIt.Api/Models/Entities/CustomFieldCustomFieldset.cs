using System;
using System.Collections.Generic;

namespace SnipeIt.Api.Models.Entities;

public partial class CustomFieldCustomFieldset
{
    public ulong Id { get; set; }

    public int CustomFieldId { get; set; }

    public int CustomFieldsetId { get; set; }

    public int Order { get; set; }

    public bool Required { get; set; }
}
