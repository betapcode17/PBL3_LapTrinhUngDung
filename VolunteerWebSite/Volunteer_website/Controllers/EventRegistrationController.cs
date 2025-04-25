using Microsoft.AspNetCore.Mvc;
using Volunteer_website.Data;
using Volunteer_website.Models;

namespace Volunteer_website.Controllers
{
    
    public class EventRegistrationController : Controller
    {
        private readonly VolunteerDbContext _context;

        public EventRegistrationController(VolunteerDbContext context)
        {
            _context = context;
        }
        [HttpGet("EventRegistration/Register/{id}")] 
        public IActionResult RegisterGet(string id)
        {
            return RedirectToAction("Detail_Event", "Home", new { id = id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            // Kiểm tra đã đăng ký chưa
            var existed = _context.EventVolunteers
             .FirstOrDefault(r => r.EventId == id && r.VolunteerId == userId );

            if (existed != null)
            {
                TempData["Message"] = "Bạn đã đăng ký sự kiện này rồi.";
                TempData.Keep("Message");
                return RedirectToAction("Detail_Event","Home", new { id = id });
            }

            var registration = new EventVolunteer
            {
                EventId = id,
                VolunteerId = userId,
                EventVolunteerDate = DateOnly.FromDateTime(DateTime.Today),
                Status = "Đang chờ duyệt" 

            };
            TempData["Message"] = null;

            _context.EventVolunteers.Add(registration);
            _context.SaveChanges();

            TempData["Message"] = "Đăng ký tham gia sự kiện thành công!";
            TempData.Keep("Message");

            return RedirectToAction("Detail_Event", "Home", new { id = id });



        }

    }
}

