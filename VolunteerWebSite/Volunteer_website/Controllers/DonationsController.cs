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
        private readonly Services.EmailService _emailService;

        public DonationsController(VolunteerManagementContext context, ILogger<DonationsController> logger, IVnPayService vnPayService, Services.EmailService emailService)
        {
            _context = context;
            _logger = logger;
            _vnPayService = vnPayService;
            _emailService = emailService;
        }

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

                if (role == "0" && !string.IsNullOrEmpty(userId))
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

                // Lấy thông tin sự kiện và người dùng để gửi email
                var eventModel = await _context.Events.FirstOrDefaultAsync(e => e.EventId == donationModel.Event_id);
                var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.VolunteerId == donationModel.Volunteer_Id);

                if (volunteer != null && !string.IsNullOrEmpty(volunteer.Email))
                {
                    // Tạo nội dung email
                    string subject = "Cảm ơn bạn đã ủng hộ sự kiện!";
                    string body = $@"
                            <h3>Xin chào {volunteer.Name},</h3>
                            <p>Cảm ơn bạn đã ủng hộ sự kiện <strong>{eventModel?.Name}</strong>!</p>
                            <p><strong>Số tiền ủng hộ:</strong> {string.Format("{0:N0} VND", donationModel.Amount)}</p>
                            <p><strong>Thời gian ủng hộ:</strong> {donation.DonationDate?.ToString("HH:mm - dd/MM/yyyy")}</p>
                            <p><strong>Lời nhắn:</strong> {donationModel.Note ?? "Không có lời nhắn"}</p>
                            <p>Chúng tôi rất trân trọng sự đóng góp của bạn để giúp sự kiện thành công!</p>
                            <p>Trân trọng,<br>Đội ngũ Volunteer Website</p>";

                    try
                    {
                        await _emailService.SendEmailAsync(volunteer.Email, subject, body);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Không gửi được email tới {volunteer.Email}");
                        TempData["Error"] = "Đã tạo donation nhưng không gửi được email xác nhận.";
                    }
                }

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
    

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            _logger.LogInformation($"PaymentCallbackVnpay: TransactionId={response.TransactionId}, Success={response.Success}");

            var donation = _context.Donations
                .Include(d => d.Volunteer)
                .Include(d => d.Event)
                .FirstOrDefault(d => d.DonationId == response.TransactionId);

            if (response.Success && donation != null && donation.EventId != null)
            {
                if (donation.Volunteer != null && !string.IsNullOrEmpty(donation.Volunteer.Email))
                {
                    string subject = "Cảm ơn bạn đã ủng hộ sự kiện!";
                    string body = $@"
                    <h3>Xin chào {donation.Volunteer.Name},</h3>
                    <p>Cảm ơn bạn đã ủng hộ sự kiện <strong>{donation.Event?.Name}</strong>!</p>
                    <p><strong>Số tiền ủng hộ:</strong> {string.Format("{0:N0} VND", donation.Amount)}</p>
                    <p><strong>Thời gian ủng hộ:</strong> {donation.DonationDate?.ToString("HH:mm - dd/MM/yyyy")}</p>
                    <p><strong>Lời nhắn:</strong> {donation.Message ?? "Không có lời nhắn"}</p>
                    <p>Chúng tôi rất trân trọng sự đóng góp của bạn để giúp sự kiện thành công!</p>
                    <p>Trân trọng,<br>Đội ngũ Volunteer Website</p>";

                    try
                    {
                        await _emailService.SendEmailAsync(donation.Volunteer.Email, subject, body);
                        _logger.LogInformation($"Email sent successfully to {donation.Volunteer.Email}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send email to {donation.Volunteer.Email}");
                        TempData["Error"] = "Thanh toán thành công nhưng không gửi được email xác nhận.";
                    }
                }
                else
                {
                    _logger.LogWarning($"No valid volunteer or email for DonationId={response.TransactionId}");
                    TempData["Error"] = "Thanh toán thành công nhưng không tìm thấy thông tin người ủng hộ.";
                }

                TempData["Message"] = "Thanh toán thành công! Cảm ơn bạn đã ủng hộ.";
                return RedirectToAction("Detail_Event", "Home", new { id = donation.EventId });
            }

            _logger.LogWarning($"Payment failed or invalid donation for TransactionId={response.TransactionId}");
            TempData["Error"] = "Thanh toán không thành công. Vui lòng thử lại.";
            var fallbackEventId = donation?.EventId ?? "";
            return RedirectToAction("Detail_Event", "Home", new { id = fallbackEventId });
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}