using Microsoft.AspNetCore.Mvc;
using Volunteer_website.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Volunteer_website.Controllers
{
    [Authorize("Volunteer")]
    public class EventRegistrationController : Controller
    {
        private readonly VolunteerManagementContext _context;

        public EventRegistrationController(VolunteerManagementContext context)
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
        public async Task<IActionResult> Register(string id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user is a registered volunteer
            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.VolunteerId == userId);

            if (volunteer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tình nguyện viên. Vui lòng đăng ký làm tình nguyện viên trước.";
                TempData.Keep("Error");
                return RedirectToAction("Detail_Event", "Home", new { id });
            }

            // Check for existing registration
            var existed = await _context.Registrations
                .FirstOrDefaultAsync(r => r.EventId == id && r.VolunteerId == volunteer.VolunteerId);

            if (existed != null)
            {
                TempData["Error"] = "Bạn đã đăng ký sự kiện này rồi.";
                TempData.Keep("Error");
                return RedirectToAction("Detail_Event", "Home", new { id });
            }

            // Generate new registration ID
            string newId = "REG0001";
            var maxId = await _context.Registrations
                .Select(e => e.RegId)
                .OrderByDescending(id => id)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(maxId) && maxId.StartsWith("REG"))
            {
                if (int.TryParse(maxId.Substring(3), out int numericPart))
                {
                    newId = $"REG{(numericPart + 1):D4}";
                }
            }

            var registration = new Registration
            {
                RegId = newId, // Use sequential ID instead of GUID
                EventId = id,
                VolunteerId = volunteer.VolunteerId,
                RegisterAt = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = "PENDING"
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký tham gia sự kiện thành công!";
            TempData.Keep("SuccessMessage");

            return RedirectToAction("Detail_Event", "Home", new { id });
        }
        [HttpGet("EventRegistration/MyRegistrations")]
        [HttpGet("EventRegistration")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("SignIn", "Account");
            }

            var registrations = await _context.Registrations
                .Where(r => r.VolunteerId == userId)
                .Include(r => r.Event)
                .Include(r => r.Evaluations) // Đảm bảo bao gồm Evaluations
                .ToListAsync();

            return View(registrations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRegistration(string regId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("SignIn", "Account");
            }

            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.RegId == regId && r.VolunteerId == userId);

            if (registration == null)
            {
                TempData["Error"] = "Không tìm thấy đăng ký.";
                TempData.Keep("Error");
                return RedirectToAction("Index");
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã hủy đăng ký thành công!";
            TempData.Keep("SuccessMessage");
            return RedirectToAction("Index");
        }
    }
}