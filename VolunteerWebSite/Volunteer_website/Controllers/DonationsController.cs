using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Volunteer_website.Models;
using Volunteer_website.ViewModels;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Filters;
using Volunteer_website.Services;
using Volunteer_website.Helpers;
namespace Volunteer_website.Controllers
{
    public class DonationsController : Controller
    {
        private readonly VolunteerManagementContext _context;
        private readonly ILogger<DonationsController> _logger;
        private readonly IVnPayService _vnPayService;
        public DonationsController(VolunteerManagementContext context, ILogger<DonationsController> logger, IVnPayService vnPayService)
        {
            _context = context;
            _logger = logger;
            _vnPayService = vnPayService;
        }

        //// GET: Donations/Create
        //[Authorize]
        //public IActionResult Create(int eventId)
        //{
        //    // Get the event from database
        //    var eventModel = _context.Events.Find(eventId);
        //    if (eventModel == null)
        //    {
        //        return NotFound();
        //    }

        //    var model = new DonationModel
        //    {
        //        EventId = eventId
        //    };

        //    // Pre-fill user info for all authenticated users
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var user = _context.Volunteers.Find(userId);

        //    if (user != null)
        //    {
        //        model.Name = user.Name;
        //        model.Email = user.Email;
        //        model.Phone = user.PhoneNumber;
        //        model.Address = user.Address;
        //    }

        //    return View(model);
        //}

        // POST: Donations/Create for Volunteer
        [OnlyVolunteer]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonationModel donationModel)
        {
            if (!ModelState.IsValid)
            {
                return View(donationModel);
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                // Kiểm tra và gán Volunteer_Id
                if (role == "1" && !string.IsNullOrEmpty(userId))
                {
                    if (!InputValidator.IsValidId(userId)) // Kiểm tra định dạng ID
                    {
                        ModelState.AddModelError("Volunteer_Id", "Invalid volunteer ID format.");
                        return View(donationModel);
                    }
                    var volunteerExists = await _context.Volunteers.AnyAsync(v => v.VolunteerId == userId);
                    if (!volunteerExists)
                    {
                        ModelState.AddModelError("Volunteer_Id", "Volunteer ID does not exist.");
                        return View(donationModel);
                    }
                    donationModel.Volunteer_Id = userId;
                }
                else
                {
                    ModelState.AddModelError("Volunteer_Id", "User is not authorized to donate as a volunteer.");
                    return View(donationModel);
                }

                // Kiểm tra các trường đầu vào
                if (string.IsNullOrEmpty(donationModel.Event_id) || !InputValidator.IsValidId(donationModel.Event_id))
                {
                    ModelState.AddModelError("Event_id", "Invalid event ID format.");
                    return View(donationModel);
                }

                if (!InputValidator.IsValidAmount(donationModel.Amount) || donationModel.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Amount must be a positive number.");
                    return View(donationModel);
                }

                if (!string.IsNullOrEmpty(donationModel.Note) && !InputValidator.IsValidFeedback(donationModel.Note))
                {
                    ModelState.AddModelError("Note", "Note contains invalid characters.");
                    return View(donationModel);
                }

                // Kiểm tra xem EventId có tồn tại không
                var eventExists = await _context.Events.AnyAsync(e => e.EventId == donationModel.Event_id);
                if (!eventExists)
                {
                    ModelState.AddModelError("Event_id", "Event does not exist.");
                    return View(donationModel);
                }

                // Tạo DonationId
                string newId = "DON0001";
                var maxId = await _context.Donations
                    .Select(e => e.DonationId)
                    .OrderByDescending(id => id)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(maxId) && maxId.StartsWith("DON")) // Sửa "D0N" thành "DON"
                {
                    if (int.TryParse(maxId.Substring(3), out int numericPart))
                    {
                        newId = $"DON{(numericPart + 1):D4}";
                    }
                }

                var donation = new Models.Donation
                {
                    DonationId = newId,
                    EventId = donationModel.Event_id,
                    VolunteerId = donationModel.Volunteer_Id,
                    DonationDate = DateTime.Now,
                    Amount = donationModel.Amount,
                    Message = donationModel.Note
                };
                _logger.LogInformation($"DonationDate before save: {donation.DonationDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null"}");

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Cảm ơn bạn đã đóng góp cho sự kiện!";
                return RedirectToAction("Detail_Event", "Home", new { id = donationModel.Event_id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while creating donation.");
                TempData["Error"] = "A database error occurred. Please try again later.";
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating donation.");
                TempData["Error"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction("Error");
            }
        }
    

        
        public IActionResult Index()
        {
            

            return View();
        }
        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }

    }
}