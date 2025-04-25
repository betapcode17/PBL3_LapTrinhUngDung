using System;
using System.Collections.Generic;

namespace Volunteer_website.Data;

public partial class EventImage
{
    public int Id { get; set; }

    public string EventId { get; set; } = null!;

    public string ImagePath { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;
    public string? Description { get; internal set; }
}
