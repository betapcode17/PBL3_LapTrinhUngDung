using Microsoft.AspNetCore.Mvc;
using Volunteer_website.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Volunteer_website.Controllers
{
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
        public IActionResult Register(string id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem userId có tương ứng với một volunteer trong bảng Volunteers không
            var volunteer = _context.Volunteers
                .FirstOrDefault(v => v.VolunteerId == userId); // Giả sử bảng Volunteers có cột UserId tham chiếu đến AspNetUsers

            if (volunteer == null)
            {
                TempData["Message"] = "Không tìm thấy thông tin tình nguyện viên. Vui lòng đăng ký làm tình nguyện viên trước.";
                TempData.Keep("Message");
                return RedirectToAction("Detail_Event", "Home", new { id = id });
            }

            // Kiểm tra xem người dùng đã đăng ký sự kiện này chưa
            var existed = _context.Registrations
                .FirstOrDefault(r => r.EventId == id && r.VolunteerId == volunteer.VolunteerId);

            if (existed != null)
            {
                TempData["Message"] = "Bạn đã đăng ký sự kiện này rồi.";
                TempData.Keep("Message");
                return RedirectToAction("Detail_Event", "Home", new { id = id });
            }

            // Tạo bản ghi Registration với volunteer_id hợp lệ
            var registration = new Registration
            {
                RegId = Guid.NewGuid().ToString().Substring(0, 5),
                EventId = id,
                VolunteerId = volunteer.VolunteerId, // Sử dụng volunteer_id từ bảng Volunteers
                RegisterAt = DateOnly.FromDateTime(DateTime.Today),
                Status = "PENDING"
            };
            TempData["Message"] = null;

            _context.Registrations.Add(registration);
            _context.SaveChanges();

            TempData["Message"] = "Đăng ký tham gia sự kiện thành công!";
            TempData.Keep("Message");

            return RedirectToAction("Detail_Event", "Home", new { id = id });
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
                TempData["Message"] = "Không tìm thấy đăng ký.";
                TempData.Keep("Message");
                return RedirectToAction("Index");
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã hủy đăng ký thành công!";
            TempData.Keep("Message");
            return RedirectToAction("Index");
        }
    }
}