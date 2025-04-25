using Volunteer_website.Data;

public partial class Event
{
    public string EventId { get; set; } = null!;
    public string? OrgId { get; set; }
    public string? TypeEventId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateOnly? DayBegin { get; set; }
    public DateOnly? DayEnd { get; set; }
    public string? Location { get; set; }
    public int? TargetMember { get; set; }
    public decimal? TargetFunds { get; set; }
    public string? ImagePath { get; set; } 
    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public virtual ICollection<EventImage> EventImages { get; set; } = new List<EventImage>();
    public virtual ICollection<EventVolunteer> EventVolunteers { get; set; } = new List<EventVolunteer>();
    public virtual Organization? Org { get; set; }
    public virtual EventType? TypeEvent { get; set; }
   
}