using System.ComponentModel.DataAnnotations;
using Volunteer_website.Data;

namespace Volunteer_website.Models
{
    public class DonationModel
    {
        public string? Volunteer_Id { get; set; }
        public string Event_id { get; set; }
        [Required]
        [Range(1000, 100000000, ErrorMessage = "Số tiền phải từ 1,000 VND trở lên")]
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public DateTime DonationDate { get; set; } = DateTime.Now;
 
    }
}
