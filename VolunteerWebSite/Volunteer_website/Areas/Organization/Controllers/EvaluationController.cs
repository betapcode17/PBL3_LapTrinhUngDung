
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Models;
using Volunteer_website.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;
using X.PagedList.Extensions;

namespace Volunteer_website.Areas.Organizations.Controllers
{
    [Area("Organization")]
    [Route("[area]/[controller]/[action]")]
    [Authorize("Org")]
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
            var orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(orgId))
            {
                return RedirectToAction("Login", "Account"); // Hoặc xử lý lỗi khác nếu không có orgId
            }

            var query = _db.Evaluations
                           .Include(e => e.Reg)
                           .AsNoTracking();

            // Lọc theo OrgId thông qua RegId và Event
            query = query.Where(e => _db.Registrations
                                       .Any(r => r.RegId == e.RegId &&
                                                 _db.Events.Any(ev => ev.EventId == r.EventId && ev.OrgId == orgId)));

            if (!string.IsNullOrEmpty(searchValue))
            {
                var matchedEventIds = _db.Events
                                        .Where(ev => ev.Name != null && ev.Name.Contains(searchValue))
                                        .Select(ev => ev.EventId)
                                        .ToList();
                var matchedVolunteerIds = _db.Volunteers
                                            .Where(v => v.Name != null && v.Name.Contains(searchValue))
                                            .Select(v => v.VolunteerId)
                                            .ToList();
                var matchedRegIds = _db.Registrations
                                       .Where(r => matchedEventIds.Contains(r.EventId) ||
                                                   matchedVolunteerIds.Contains(r.VolunteerId))
                                       .Select(r => r.RegId)
                                       .ToList();
                query = query.Where(e => matchedRegIds.Contains(e.RegId));
            }

            var evaluations = query
                              .OrderBy(e => e.EvaluationId)
                              .ToPagedList(pageNumber, pageSize);

            // Lấy danh sách RegId từ evaluations đã phân trang
            var regIds = evaluations.Select(e => e.RegId).Distinct().ToList();
            var registrations = _db.Registrations
                                  .Where(r => regIds.Contains(r.RegId))
                                  .AsNoTracking()
                                  .ToList();

            // Lấy danh sách VolunteerId và EventId từ registrations
            var volunteerIds = registrations.Select(r => r.VolunteerId).Distinct().ToList();
            var eventIds = registrations.Select(r => r.EventId).Distinct().ToList();

            // Lấy thông tin Volunteers và Events dưới dạng Dictionary
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
                dateOfBirth = volunteer.DateOfBirth?.ToString("yyyy-MM-dd"),
                gender = volunteer.Gender,
                imagePath = volunteer.ImagePath ?? "/default-profile.png",
                address = volunteer.Address,
                age = CalculateAge(volunteer.DateOfBirth?.ToDateTime(TimeOnly.MinValue))
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
            string newId = "EVL0001";
            var maxId = _db.Evaluations
                .Select(e => e.EvaluationId)
                .OrderByDescending(id => id)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(maxId) && maxId.StartsWith("EVL"))
            {
                if (int.TryParse(maxId.Substring(3), out int numericPart))
                {
                    newId = $"EVL{(numericPart + 1):D4}";
                }
            }

            var evaluationModel = new Evaluation
            {
                EvaluationId = newId,
                IsCompleted = false
            };

            string orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
            var volunteers = _db.Volunteers
                .Where(v => _db.Registrations.Any(r =>
                    r.VolunteerId == v.VolunteerId &&
                    r.Event.OrgId == orgId))
                .OrderBy(v => v.Name)
                .Select(v => new SelectListItem
                {
                    Value = v.VolunteerId,
                    Text = v.Name
                })
                .ToList();

            ViewBag.Volunteers = volunteers;
            ViewBag.OrgId = orgId;
            return View(evaluationModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EvaluationId,RegId,Feedback,IsCompleted")] Evaluation model)
        {
            try
            {
                if (!ModelState.IsValid || !ValidateEvaluation(model))
                {
                    ReloadVolunteers();
                    return View(model);
                }

                if (_db.Evaluations.Any(e => e.EvaluationId == model.EvaluationId))
                {
                    TempData["Error"] = "Mã đánh giá đã tồn tại.";
                    ReloadVolunteers();
                    return View(model);
                }

                // Kiểm tra xem RegId đã tồn tại trong bảng Evaluations chưa
                if (await _db.Evaluations.AnyAsync(e => e.RegId == model.RegId))
                {
                    TempData["Error"] = "Bạn đã đánh giá đơn đăng ký của tình nguyện viên này rồi.";
                    ReloadVolunteers();
                    return View(model);
                }

                var evaluation = new Evaluation
                {
                    EvaluationId = model.EvaluationId,
                    RegId = model.RegId,
                    Feedback = model.Feedback,
                    EvaluatedAt = DateTime.Now,
                    IsCompleted = model.IsCompleted
                };

                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _db.Evaluations.Add(evaluation);
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                TempData["SuccessMessage"] = "Tạo đánh giá thành công!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                TempData["Error"] = $"Lỗi cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}";
                ReloadVolunteers();
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi hệ thống: {ex.Message}";
                ReloadVolunteers();
                return View(model);
            }
        }
        #endregion

        #region Sửa đánh giá
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (!InputValidator.IsValidEvaluationId(id))
            {
                TempData["Error"] = "Mã đánh giá không hợp lệ.";
                return RedirectToAction("Index");
            }

            string orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
            var evaluation = _db.Evaluations
                .Include(e => e.Reg)
                .ThenInclude(r => r.Event)
                .ThenInclude(e => e.Org)
                .Include(e => e.Reg)
                .ThenInclude(r => r.Volunteer)
                .FirstOrDefault(e => e.EvaluationId == id);

            if (evaluation == null)
            {
                TempData["Error"] = "Không tìm thấy đánh giá.";
                return RedirectToAction("Index");
            }

            if (evaluation.Reg?.Event?.OrgId != orgId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa đánh giá này.";
                return Forbid();
            }

            var volunteers = _db.Volunteers
                .Where(v => _db.Registrations.Any(r =>
                    r.VolunteerId == v.VolunteerId &&
                    r.Event.OrgId == orgId))
                .OrderBy(v => v.Name)
                .Select(v => new SelectListItem
                {
                    Value = v.VolunteerId,
                    Text = v.Name
                })
                .ToList();

            ViewBag.Volunteers = volunteers;
            ViewBag.OrgId = orgId;
            ViewBag.SelectedVolunteerId = evaluation.Reg?.VolunteerId;

            // Đổ dữ liệu chi tiết của Volunteer đã chọn lên ViewBag
            if (evaluation.Reg?.Volunteer != null)
            {
                ViewBag.SelectedVolunteer = new
                {
                    VolunteerId = evaluation.Reg.VolunteerId,
                    Name = evaluation.Reg.Volunteer.Name,
                    Email = evaluation.Reg.Volunteer.Email,
                    PhoneNumber = evaluation.Reg.Volunteer.PhoneNumber,
                    Gender = evaluation.Reg.Volunteer.Gender, // Đảm bảo là bool?
                    DateOfBirth = evaluation.Reg.Volunteer.DateOfBirth,
                    ImagePath = evaluation.Reg.Volunteer.ImagePath ?? "/OrgLayout/assets/images/pic-1.jpg"
                };
            }

            // Đổ dữ liệu của Event đã chọn lên ViewBag
            if (evaluation.Reg?.Event != null)
            {
                ViewBag.SelectedEvent = new
                {
                    RegId = evaluation.RegId,
                    EventId = evaluation.Reg.EventId,
                    Name = evaluation.Reg.Event.Name
                };
            }

            return View(evaluation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("EvaluationId,RegId,Feedback,IsCompleted")] Evaluation model)
        {
            try
            {
                if (!ModelState.IsValid || !ValidateEvaluation(model))
                {
                    ReloadVolunteers();
                    ViewBag.SelectedVolunteerId = _db.Registrations
                        .FirstOrDefault(r => r.RegId == model.RegId)?.VolunteerId;
                    return View(model);
                }

                var evaluation = _db.Evaluations
                    .Include(e => e.Reg)
                    .ThenInclude(r => r.Event)
                    .FirstOrDefault(e => e.EvaluationId == model.EvaluationId);

                if (evaluation == null)
                {
                    TempData["Error"] = "Không tìm thấy đánh giá.";
                    ReloadVolunteers();
                    return View(model);
                }

                string orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
                if (evaluation.Reg?.Event?.OrgId != orgId)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa đánh giá này.";
                    ReloadVolunteers();
                    return Forbid();
                }

                evaluation.RegId = model.RegId;
                evaluation.Feedback = model.Feedback;
                evaluation.IsCompleted = model.IsCompleted;
                evaluation.EvaluatedAt = DateTime.Now;

                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _db.Update(evaluation);
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                TempData["SuccessMessage"] = "Cập nhật đánh giá thành công!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                TempData["Error"] = $"Lỗi cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}";
                ReloadVolunteers();
                ViewBag.SelectedVolunteerId = _db.Registrations
                    .FirstOrDefault(r => r.RegId == model.RegId)?.VolunteerId;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi hệ thống: {ex.Message}";
                ReloadVolunteers();
                ViewBag.SelectedVolunteerId = _db.Registrations
                    .FirstOrDefault(r => r.RegId == model.RegId)?.VolunteerId;
                return View(model);
            }
        }
        #endregion

        #region Xóa đánh giá
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (!InputValidator.IsValidEvaluationId(id))
                {
                    return Json(new { success = false, message = "Mã đánh giá không hợp lệ." });
                }

                var evaluation = await _db.Evaluations
                    .FirstOrDefaultAsync(e => e.EvaluationId == id);

                if (evaluation == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đánh giá." });
                }

                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _db.Evaluations.Remove(evaluation);
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Xóa đánh giá thành công!" });
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (DbUpdateException dbEx)
            {
                return Json(new { success = false, message = $"Lỗi cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
        #endregion

        #region Gửi email đánh giá
        public IActionResult SendEmail(string id)
        {
            try
            {
                var evaluation = _db.Evaluations
                    .Include(e => e.Reg)
                    .AsNoTracking()
                    .FirstOrDefault(e => e.EvaluationId == id);

                if (evaluation == null)
                {
                    TempData["Error"] = "Không tìm thấy đánh giá.";
                    return RedirectToAction("Index");
                }

                var registration = _db.Registrations
                    .AsNoTracking()
                    .FirstOrDefault(r => r.RegId == evaluation.RegId);

                if (registration == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn đăng ký.";
                    return RedirectToAction("Index");
                }

                var volunteer = _db.Volunteers
                    .AsNoTracking()
                    .FirstOrDefault(v => v.VolunteerId == registration.VolunteerId);

                if (volunteer == null)
                {
                    TempData["Error"] = "Không tìm thấy tình nguyện viên.";
                    return RedirectToAction("Index");
                }

                string toEmail = "quocdat19991712@gmail.com";
                string volunteerName = volunteer.Name ?? "Tình nguyện viên";
                string feedback = evaluation.Feedback ?? "Không có nhận xét.";
                string statusText = evaluation.IsCompleted ? "Đã hoàn thành xuất sắc" : "Đã tham gia";

                if (!toEmail.Contains("@") || !toEmail.Contains("."))
                {
                    TempData["Error"] = "Địa chỉ email không hợp lệ.";
                    return RedirectToAction("Index");
                }

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

                smtpClient.Send(mail);
                TempData["SuccessMessage"] = $"Đã gửi email thông báo thành công đến {volunteerName}!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi hệ thống: {ex.Message}. Vui lòng liên hệ quản trị viên.";
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Lấy Thông tin event từ tình nguyện viên
        [HttpGet]
        public IActionResult GetEventsByVolunteer(string volunteerId)
        {
            if (string.IsNullOrEmpty(volunteerId))
            {
                return Json(new { success = false, message = "Yêu cầu Volunteer ID." });
            }

            var orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
            var events = _db.Registrations
                .Where(r => r.VolunteerId == volunteerId && r.Event.OrgId == orgId)
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

        private bool ValidateEvaluation(Evaluation model)
        {
            bool isValid = true;

            if (!InputValidator.IsValidEvaluationId(model.EvaluationId))
            {
                TempData["Error"] = "Mã đánh giá không hợp lệ. Phải bắt đầu bằng 'EVL' và theo sau là 4 chữ số.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(model.RegId) || !InputValidator.IsValidId(model.RegId))
            {
                TempData["Error"] = "Mã đăng ký không hợp lệ hoặc chưa được chọn.";
                isValid = false;
            }
            else
            {
                string orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
                var registration = _db.Registrations
                    .Include(r => r.Event)
                    .FirstOrDefault(r => r.RegId == model.RegId && r.Event.OrgId == orgId);
                if (registration == null)
                {
                    TempData["Error"] = "Mã đăng ký không tồn tại hoặc không thuộc tổ chức của bạn.";
                    isValid = false;
                }
            }

            if (string.IsNullOrEmpty(model.Feedback))
            {
                TempData["Error"] = "Phản hồi không được để trống.";
                isValid = false;
            }
            else if (!InputValidator.IsValidFeedback(model.Feedback))
            {
                TempData["Error"] = "Phản hồi không hợp lệ. Vui lòng chỉ sử dụng chữ cái (bao gồm tiếng Việt), số, khoảng trắng, dấu chấm hoặc dấu phẩy.";
                isValid = false;
            }

            return isValid;
        }

        private void ReloadVolunteers()
        {
            string orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
            var volunteers = _db.Volunteers
                .Where(v => _db.Registrations.Any(r =>
                    r.VolunteerId == v.VolunteerId &&
                    r.Event.OrgId == orgId))
                .OrderBy(v => v.Name)
                .Select(v => new SelectListItem
                {
                    Value = v.VolunteerId,
                    Text = v.Name
                })
                .ToList();

            ViewBag.Volunteers = volunteers;
            ViewBag.OrgId = orgId;
        }
    }
}