using Microsoft.AspNetCore.Mvc;
using Volunteer_website.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Diagnostics;
using X.PagedList.Extensions;

namespace Volunteer_website.Areas.Organization.Controllers
{
    [Area("Organization")]
    [Route("[area]/[controller]/[action]")] // Sửa lại route template
    public class EvaluationController : Controller
    {
        private readonly VolunteerManagementContext _db;
    
        public EvaluationController(VolunteerManagementContext db)
        {
            _db = db;
        }
        #region Xem danh sách đánh giá
        public IActionResult Index(int? page, string searchValue)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var query = _db.Evaluations
                           .Include(e => e.Reg)
                           .AsNoTracking();
            if (!string.IsNullOrEmpty(searchValue))
            {
                var matchedEventIds = _db.Events
                                         .Where(ev => ev.Name.Contains(searchValue))
                                         .Select(ev => ev.EventId)
                                         .ToList();
                var matchedVolunteerIds = _db.Volunteers
                                             .Where(v => v.Name.Contains(searchValue))
                                             .Select(v => v.VolunteerId)
                                             .ToList();
                var matchedRegIds = _db.Registrations
                                       .Where(r =>
                                            matchedEventIds.Contains(r.EventId) ||
                                            matchedVolunteerIds.Contains(r.VolunteerId))
                                       .Select(r => r.RegId)
                                       .ToList();
                query = query.Where(e => matchedRegIds.Contains(e.RegId));
            }
            var evaluations = query
                              .OrderBy(e => e.EvaluationId)
                              .ToPagedList(pageNumber, pageSize);
         
            var regIds = evaluations.Select(e => e.RegId).Distinct().ToList();
            var registrations = _db.Registrations
                                   .Where(r => regIds.Contains(r.RegId))
                                   .AsNoTracking()
                                   .ToList();

            var volunteerIds = registrations.Select(r => r.VolunteerId).Distinct().ToList();
            var eventIds = registrations.Select(r => r.EventId).Distinct().ToList();

            var volunteers = _db.Volunteers
                                .Where(v => volunteerIds.Contains(v.VolunteerId))
                                .AsNoTracking()
                                .ToDictionary(v => v.VolunteerId);

            var events = _db.Events
                            .Where(e => eventIds.Contains(e.EventId))
                            .AsNoTracking()
                            .ToDictionary(e => e.EventId);

            ViewBag.Volunteers = volunteers;
            ViewBag.Events = events;
            ViewBag.SearchValue = searchValue;

            return View(evaluations);
        }

        #endregion
        #region Lấy thông tin tình nguyện viên
        [HttpGet]
        public IActionResult GetVolunteerDetails(string id)
        {
            var volunteer = _db.Volunteers
                            .FirstOrDefault(v => v.VolunteerId == id);

            if (volunteer == null)
            {
                return NotFound();
            }

            return Json(new
            {
                name = volunteer.Name,
                email = volunteer.Email,
                phoneNumber = volunteer.PhoneNumber,
                dateOfBirth = volunteer.DateOfBirth?.ToString("yyyy-MM-dd"), // Format ngày tháng
                gender = volunteer.Gender, // true/false hoặc null
                imagePath = volunteer.ImagePath ?? "/default-profile.png", // Fallback image nếu null
                address = volunteer.Address,
                age = CalculateAge(volunteer.DateOfBirth?.ToDateTime(TimeOnly.MinValue)) // Thêm tuổi nếu cần
            });
        }
        private int? CalculateAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return null;

            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Value.Year;

            if (dateOfBirth.Value.Date > today.AddYears(-age))
                age--;

            return age;
        }
        #endregion


        #region Tạo đánh giá
        [HttpGet]
        public IActionResult Create()
        {
          
            string newId = "EVL001"; 

            var maxId = _db.Evaluations
                .Select(e => e.EvaluationId)
                .OrderByDescending(id => id)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(maxId))
            {
              
                if (int.TryParse(maxId.Substring(3), out int numericPart))
                {
                    
                    newId = $"EVL{(numericPart + 1).ToString("D3")}";
                }
            }

            var evaluationModel = new Evaluation
            {
                EvaluationId = newId 
            };

            var registrations = _db.Registrations
                .Include(r => r.Volunteer)
                .Include(r => r.Event)
                .ToList();

            ViewBag.Registrations = registrations;

            return View(evaluationModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("EvaluationId,RegId,Feedback,IsCompleted")] Evaluation model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Ghi log lỗi nếu có
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .ToList();

                    foreach (var error in errors)
                    {
                        Debug.WriteLine($"Error: {error.ErrorMessage}");
                    }

                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.RegId))
                {
                    ModelState.AddModelError("RegId", "Mã đăng ký không được để trống.");
                    return View(model);
                }

                // Tìm đăng ký
                var registration = _db.Registrations
                    .Include(r => r.Volunteer)
                    .Include(r => r.Event)
                    .FirstOrDefault(r => r.RegId == model.RegId);

                if (registration == null)
                {
                    ModelState.AddModelError("RegId", "Mã đăng ký không tồn tại trong hệ thống.");
                    return View(model);
                }

                // Lấy danh sách EvaluationId và tạo ID mới
                var evaluationIds = _db.Evaluations
                    .Select(e => e.EvaluationId)
                    .ToList();

                string lastId = evaluationIds
                    .Where(id => id.StartsWith("EVL"))
                    .OrderByDescending(id => id)
                    .FirstOrDefault();

                string newId;
                if (!string.IsNullOrEmpty(lastId) && lastId.Length > 3 && int.TryParse(lastId.Substring(3), out int lastNumber))
                {
                    newId = $"EVL{(lastNumber + 1):D3}";
                }
                else
                {
                    newId = "EVL001";
                }

                // Tạo bản ghi mới
                var evaluation = new Evaluation
                {
                    EvaluationId = newId,
                    RegId = model.RegId,
                    Feedback = model.Feedback,
                    EvaluatedAt = DateTime.Now,
                    IsCompleted = model.IsCompleted
                };

                // Bắt đầu giao dịch và lưu dữ liệu
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.Evaluations.Add(evaluation);
                        _db.SaveChanges();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                TempData["SuccessMessage"] = "Tạo đánh giá thành công!";
                return RedirectToAction("Index", "Evaluation");
            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError("", $"Lỗi database: {dbEx.InnerException?.Message ?? dbEx.Message}");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
                return View(model);
            }
        }



        #endregion

        #region Gửi email

        #region Gửi email đánh giá
        public IActionResult SendEmail(string id)
        {
            try
            {
                // Lấy đánh giá
                var evaluation = _db.Evaluations
                    .Include(e => e.Reg)
                    .AsNoTracking()
                    .FirstOrDefault(e => e.EvaluationId == id);

                if (evaluation == null)
                {
                    TempData["Error"] = "Không tìm thấy đánh giá.";
                    return RedirectToAction("Index");
                }

                // Lấy đăng ký
                var registration = _db.Registrations
                    .AsNoTracking()
                    .FirstOrDefault(r => r.RegId == evaluation.RegId);

                if (registration == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn đăng ký.";
                    return RedirectToAction("Index");
                }

                // Lấy tình nguyện viên
                var volunteer = _db.Volunteers
                    .AsNoTracking()
                    .FirstOrDefault(v => v.VolunteerId == registration.VolunteerId);

                if (volunteer == null)
                {
                    TempData["Error"] = "Không tìm thấy tình nguyện viên.";
                    return RedirectToAction("Index");
                }

                // Thông tin email
                string toEmail = "quocdat19991712@gmail.com";
                string volunteerName = volunteer.Name ?? "Tình nguyện viên";
                string feedback = evaluation.Feedback ?? "Không có nhận xét.";
                string statusText = evaluation.IsCompleted ? "Đã hoàn thành xuất sắc" : "Đã tham gia";

                // Kiểm tra định dạng email
                if (!toEmail.Contains("@") || !toEmail.Contains("."))
                {
                    TempData["Error"] = "Địa chỉ email không hợp lệ.";
                    return RedirectToAction("Index");
                }

                // Nội dung email
                string subject = "Kết quả đánh giá hoạt động tình nguyện";
                string body = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <h2 style='color: #2c3e50;'>Xin chào {volunteerName},</h2>
    <p>Kết quả đánh giá hoạt động vừa qua của bạn:</p>
    <p><strong>Trạng thái:</strong> <span style='color: {(evaluation.IsCompleted ? "#27ae60" : "#e74c3c")};'>{statusText}</span></p>
    <p><strong>Nhận xét:</strong> {feedback}</p>
    <p style='margin-top: 30px;'>Cảm ơn bạn đã đóng góp cho chương trình!</p>
    <div style='margin-top: 20px; padding-top: 20px; border-top: 1px solid #eee;'>
        <p>Trân trọng,</p>
        <p><strong>Ban tổ chức chương trình tình nguyện</strong></p>
    </div>
</div>";

                // Gửi email
                var fromEmail = "volunteer.web.pbl3@gmail.com";
                var displayName = "VolunteerWebAdmin_HDB";
                var appPassword = "dkep waiy kymb uhnl";

                using var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, displayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);

                using var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail, appPassword)
                };

                Console.WriteLine($"Đang gửi email đến: {toEmail}...");
                smtpClient.Send(mail);
                Console.WriteLine("✅ Gửi email thành công.");
              
                TempData["SuccessMessage"] = $"Đã gửi email thông báo thành công đến {volunteerName}!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi email: {ex.Message}");
                Console.WriteLine($"Chi tiết lỗi: {ex.StackTrace}");
                TempData["Error"] = $"Lỗi hệ thống: {ex.Message}. Vui lòng liên hệ quản trị viên.";
            }

            return RedirectToAction("Index");
        }
        #endregion


        #endregion


        #region Lấy Thông tin event từ tình nguyện viên
        [HttpGet]
        public IActionResult GetEventsByVolunteer(string volunteerId)
        {
            if (string.IsNullOrEmpty(volunteerId))
            {
                return Json(new { success = false, message = "Yêu cầu Volunteer ID." });
            }

            var orgID = User?.Identity?.IsAuthenticated == true
                ? User.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "ORG1"
                : "ORG1";

            var events = _db.Registrations
                .Where(r => r.VolunteerId == volunteerId && r.Event.OrgId == orgID)
                .Select(r => new
                {
                    regId = r.RegId,
                    eventId = r.Event.EventId,
                    eventName = r.Event.Name,
                })
                .OrderBy(x => x.eventName)
                .ToList();

            return Json(events);
        }
        #endregion
    }
}