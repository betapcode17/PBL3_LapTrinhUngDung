using System;
using System.Collections.Generic;

namespace Volunteer_website.Data;

public partial class Registration
{
    public string RegId { get; set; } = null!;

    public string? VolunteerId { get; set; }

    public string? EventId { get; set; }

    public string? Status { get; set; }

    public virtual Event? Event { get; set; }

    public virtual Volunteer? Volunteer { get; set; }
}
