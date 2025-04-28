using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volunteer_website.Models;
using Volunteer_website.Areas.Admin.Data;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Azure.Core;
namespace Volunteer_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/HomeAdmin")]
    public class HomeAdminController : Controller
    {
        private readonly VolunteerManagementContext _db;

        public HomeAdminController(VolunteerManagementContext db)
        {
            _db = db;
        }

        #region Trang chủ
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            var events = _db.Events.Count();
            var totalAmount = _db.Donations.AsNoTracking()
                                   .Sum(d => d.Amount);
            var volunteers = _db.Volunteers.Count();

            var currentYear = DateTime.Now.Year;
            var startYear = currentYear - 5;
            int[] eventsByYear = new int[6];

            for (int i = 0; i < 6; i++)
            {
                var year = startYear + i;
                eventsByYear[i] = _db.Events.Count(e => e.DayBegin.HasValue && e.DayBegin.Value.Year == year);
            }

            ViewBag.Events = events;
            ViewBag.Donations = totalAmount;
            ViewBag.Volunteers = volunteers;
            ViewBag.EventsByYear = eventsByYear;

            return View();
        }
        #endregion

        #region Hiển thị sự kiện
        [Route("Event")]
        public IActionResult Event(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var lstEvent = _db.Events.AsNoTracking()
                            .OrderBy(x => x.EventId)
                            .ToPagedList(pageNumber, pageSize);
            return View(lstEvent);
        }
        #endregion

        #region Duyệt sự kiện 

        [HttpPost]
        [Route("acceptEvent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> acceptEvent([FromBody] EventRequest request)
        {
            try
            {
                //var firstEvent = await _db.Events.FirstOrDefaultAsync(); ;
                //Console.WriteLine(firstEvent.EventId);
                var EventId = request.EventId;
                var existingEvent = _db.Events.FirstOrDefault(ev => ev.EventId == EventId);
                if (existingEvent == null)
                {
                    return Json(new { success = false, message = "Event not found" });
                }

                // chuyển trạng thái status
                existingEvent.Status = "ACCEPT";

                _db.Update(existingEvent);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Event accepted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("rejectEvent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> rejectEvent([FromBody] EventRequest request)
        {
            try
            {
                var existingEvent = _db.Events.FirstOrDefault(ev => ev.EventId == request.EventId);
                if (existingEvent == null)
                {
                    return Json(new { success = false, message = "Event not found" });
                }

                // chuyển trạng thái status
                existingEvent.Status = "REJECT";

                _db.Update(existingEvent);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Event rejected successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Danh sách user
        [Route("Volunteers")]
        public IActionResult Volunteers(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            // Lấy danh sách đăng ký kèm theo thông tin Volunteer và Event
            //var lstRegistered = _db.Registrations
            //                       .Include(r => r.Volunteer) // Load dữ liệu Volunteer
            //                       .Include(r => r.Event) // Load dữ liệu Event
            //                       .AsNoTracking()
            //                       .OrderBy(x => x.EventId)
            //                       .ToPagedList(pageNumber, pageSize);

            var listUser = _db.Volunteers.AsNoTracking()
                                                    .ToPagedList(pageNumber, pageSize);
            return View(listUser);
        }

        #endregion

        #region Xem chi tiết sự kiện 
        [Route("GetEventDetails")]
        [HttpGet]
        public IActionResult GetEventDetails(string id)
        {
            try
            {
                var events = _db.Events.FirstOrDefault(v => v.EventId == id);
                var registrationCount = _db.Registrations.Count(r => r.EventId == id);
                var donationCount = _db.Donations.Count(d => d.EventId == id);
                var totalAmount = _db.Donations.Where(d => d.EventId == id).Sum(d => d.Amount);

                if (events == null)
                {
                    return Json(new { success = false, message = "Event not found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        eventId = events.EventId,
                        orgId = events.OrgId,
                        typeEventName = events.TypeEventName,
                        name = events.Name,
                        description = events.Description,
                        dayBegin = events.DayBegin?.ToString("dd/MM/yyyy"),
                        dayEnd = events.DayEnd?.ToString("dd/MM/yyyy"),
                        location = events.Location,
                        targetMember = events.TargetMember,
                        targetFunds = events.TargetFunds,
                        imagePath = events.ImagePath,
                        listImg = events.ListImg,
                        status = events.Status,
                        registrationCount,
                        donationCount,
                        totalAmount
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while fetching event details",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Thêm sự kiện
        [Route("CreateEvent")]
        [HttpGet]
        public IActionResult CreateEvent()
        {
            var orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            ViewBag.OrgId = orgId;

            // Lấy EventId lớn nhất từ database
            var lastEvent = _db.Events
                .OrderByDescending(e => e.EventId)
                .FirstOrDefault();

            string newEventId = "EVT1"; // Giá trị mặc định nếu không có event nào
            if (lastEvent != null)
            {
                // Giả sử EventId có định dạng "EVT" + số (EVT1, EVT2, ...)
                string lastIdNumber = lastEvent.EventId.Replace("EVT", "");
                if (int.TryParse(lastIdNumber, out int number))
                {
                    newEventId = $"EVT{number + 1}";
                }
            }

            // Tạo model với EventId tự động và OrgId
            var model = new Event
            {
                EventId = newEventId,
                OrgId = orgId
            };

            return View(model);
        }

        [Route("CreateEvent")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(
                [Bind("EventId,OrgId,type_event_name,Name,Description,DayBegin,DayEnd,Location,TargetMember,TargetFunds,Status")]
                Event eventModel,
                IFormFile imagePath,  // Chú ý tên tham số
                IEnumerable<IFormFile> listImg)  // Chú ý tên tham số
        {

            Console.WriteLine($"Name: {eventModel?.Name}");
            Console.WriteLine($"DayEnd: {eventModel?.DayEnd}");
            if (!ModelState.IsValid)
            {
                // Log các lỗi model state
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    TempData["ErrorMessage"] = "Đã gặp lỗi.";
                    Console.WriteLine(error.ErrorMessage);
                }
                return View("CreateEvent", eventModel);
            }

            if(eventModel == null)
            {
                TempData["ErrorMessage"] = "Đã gặp lỗi không truyền được dữ liệu để xử lý.";
                return View("CreateEvent", eventModel);
            }

            // Xử lý ảnh chính (không bắt buộc)
            if (imagePath != null && imagePath.Length > 0)  // Chỉ xử lý nếu có file
            {
                var fileName = Path.GetFileName(imagePath.FileName);
                var filePath = Path.Combine("wwwroot/uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagePath.CopyToAsync(stream);
                }
                eventModel.ImagePath = "/uploads/" + fileName;
            }

            // Xử lý ảnh phụ (không bắt buộc)
            if (listImg != null && listImg.Any())  // Chỉ xử lý nếu có ít nhất 1 file
            {
                var imagePaths = new List<string>();
                foreach (var file in listImg)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine("wwwroot/uploads", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        imagePaths.Add("/uploads/" + fileName);
                    }
                }
                eventModel.ListImg = string.Join(",", imagePaths);
            }

            if(eventModel.DayBegin > eventModel.DayEnd)
            {
                TempData["ErrorMessage"] = "Đã gặp lỗi không truyền được dữ liệu để xử lý.";
                return View("CreateEvent", eventModel);
            }

            if(eventModel.TargetFunds < 0)
            {
                TempData["ErrorMessage"] = "Target Fund cannot be negative.";
                return View("CreateEvent", eventModel);
            }

            if (eventModel.TargetMember < 0)
            {
                TempData["ErrorMessage"] = "Target Member cannot be negative.";
                return View("CreateEvent", eventModel);
            }
            // Xử lý lưu vào database
            if (ModelState.IsValid)
            {
                _db.Add(eventModel);
                await _db.SaveChangesAsync();
                return RedirectToAction("Event");
            }

            // Nếu có lỗi, hiển thị lại form
            return View(eventModel);
        }

        #endregion

        #region Xóa sự kiện
        [Route("DeleteEvent")]
        [HttpPost]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            TempData["Message"] = "";

            var registrations = _db.Registrations.Where(x => x.EventId == id).ToList();
            var donations = _db.Donations.Where(x => x.EventId == id).ToList();

            if (registrations.Count > 0 || donations.Count > 0)
            {
                TempData["Message"] = "Can not delete";
                return RedirectToAction("Event");
            }

            var eventToDelete =await _db.Events.FirstOrDefaultAsync(ev => ev.EventId == id);
            if (eventToDelete != null)
            {
                try
                {
                    // Logic xóa
                    _db.Events.Remove(eventToDelete);
                    _db.SaveChanges();

                    // Trả về JSON thay vì Redirect
                    return Json(new
                    {
                        success = true,
                        message = "Xóa thành công"
                    });
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            }
            return RedirectToAction("Event");
        }

        #endregion

        #region Cập nhật trạng thái người tham gia
        [HttpGet]
        [Route("UpdateStatus")]
        public IActionResult UpdateStatus(string regId, bool status)
        {
            try
            {
                var registration = _db.Registrations.FirstOrDefault(r => r.RegId == regId);
                if (registration == null)
                {
                    return Json(new { success = false, message = "Registration not found" });
                }

                registration.Status = status;
                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = status ? "Registration approved" : "Registration rejected"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
        #endregion

        #region Xem chi tiết người tham gia
        [Route("GetVolunteerDetails")]
        [HttpGet]
        public IActionResult GetVolunteerDetails(string id)
        {
            try
            {
                var volunteer = _db.Volunteers
                    .FirstOrDefault(v => v.VolunteerId == id);

                if (volunteer == null)
                {
                    return Json(new { success = false, message = "Volunteer not found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        volunteerId = volunteer.VolunteerId,
                        name = volunteer.Name,
                        email = volunteer.Email,
                        phoneNumber = volunteer.PhoneNumber,
                        dateOfBirth = volunteer.DateOfBirth?.ToString("dd/MM/yyyy"),
                        gender = volunteer.Gender.HasValue ?
                            (volunteer.Gender.Value ? "Male" : "Female") : "Not specified",
                        imagePath = volunteer.ImagePath,
                        address = volunteer.Address
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while fetching volunteer details",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Xem danh sách người Donation
        [Route("GetDonatedVolunteer")]
        public IActionResult GetDonatedVolunteer(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var lstDonated = _db.Donations.Include(r => r.Volunteer)
                                          .Include(r => r.Event)
                                          .AsNoTracking()
                                          .OrderBy(x => x.EventId)
                                          .ToPagedList(pageNumber, pageSize);
            return View(lstDonated);
        }
        #endregion

        #region Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account", new { area = "" });
        }
        #endregion
    }
}
