using System;
using System.Collections.Generic;

namespace Volunteer_website.Data;

public partial class Volunteer 
{

    public string VolunteerId { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public bool Gender { get; set; } = true;

    public string? ImagePath { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
