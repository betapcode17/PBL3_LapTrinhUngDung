using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volunteer_website.Data;
public partial class Event : IValidatableObject
{
    [Key]
    [Required]
    public string EventId { get; set; } = null!;

    public string? OrgId { get; set; }

    [Required(ErrorMessage = "Event Type is required")]
    [Column("type_event_name")] // ← Rõ ràng chỉ định tên cột
    public string? type_event_name { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Start Date is required")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? DayBegin { get; set; }

    [Required(ErrorMessage = "End Date is required")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? DayEnd { get; set; }

    [Required(ErrorMessage = "Location is required")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Target Members is required")]
    public int? TargetMember { get; set; }

    [Required(ErrorMessage = "Target Funds is required")]
    public int? TargetFunds { get; set; }

    public string? ImagePath { get; set; }  // Dấu ? để nullable
    public string? ListImg { get; set; }    // Dấu ? để nullable

    public bool? Status { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual Organization? Org { get; set; }

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DayBegin.HasValue && DayEnd.HasValue && DayEnd < DayBegin)
        {
            yield return new ValidationResult(
                "Ngày kết thúc phải muộn hơn hoặc bằng ngày bắt đầu.",
                new[] { nameof(DayEnd) }
            );
        }
    }
}