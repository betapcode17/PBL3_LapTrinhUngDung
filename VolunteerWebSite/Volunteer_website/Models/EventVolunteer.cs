using System;
using System.Collections.Generic;

namespace Volunteer_website.Data;

public partial class EventVolunteer
{
    public string EventId { get; set; } = null!;

    public string VolunteerId { get; set; } = null!;

    public DateOnly? EventVolunteerDate { get; set; }

    public string? Status { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Volunteer Volunteer { get; set; } = null!;
}
