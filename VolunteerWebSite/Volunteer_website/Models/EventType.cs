using System;
using System.Collections.Generic;

namespace Volunteer_website.Data;

public partial class EventType
{
    public string TypeEventId { get; set; } = null!;

    public string? NameType { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
