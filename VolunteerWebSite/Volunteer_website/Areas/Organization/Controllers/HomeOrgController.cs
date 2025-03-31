
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Models;
using X.PagedList.Extensions;

namespace Volunteer_website.Areas.Organization.Controllers
{
    [Area("Organization")]
    [Route("Organization")]
    [Route("Organization/HomeOrg")]
    public class HomeOrgController : Controller
    {
        private readonly VolunteerManagementContext _db;

        public HomeOrgController(VolunteerManagementContext db)
        {
            _db = db;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
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
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(eventModel);
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

       #region Chỉnh sửa sự kiện
         [Route("EditEvent/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditEvent(string id)
        {
            var orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var eventModel = await _db.Events.FindAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }
            ViewBag.OrgId = eventModel.OrgId; // Truyền dữ liệu tổ chức vào View
            return View(eventModel);
        }

        [Route("EditEvent/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(Event model, IFormFile? imagePath, IFormFileCollection? listImg)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OrgId = model.OrgId;
                return View(model);
            }

            var existingEvent = await _db.Events.FindAsync(model.EventId);
            if (existingEvent == null)
            {
                return NotFound();
            }

            // Cập nhật dữ liệu từ form
            existingEvent.Name = model.Name;
            existingEvent.TargetMember = model.TargetMember;
            existingEvent.Status = model.Status;
            existingEvent.type_event_name = model.type_event_name;
            existingEvent.DayBegin = model.DayBegin;
            existingEvent.DayEnd = model.DayEnd;
            existingEvent.Location = model.Location;
            existingEvent.TargetFunds = model.TargetFunds;
            existingEvent.Description = model.Description;

            // Xử lý ảnh nếu có upload
            if (imagePath != null)
            {
                existingEvent.ImagePath = await SaveImage(imagePath);
            }

            _db.Update(existingEvent);
            await _db.SaveChangesAsync();

            return RedirectToAction("Event");
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return string.Empty;
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/uploads/" + uniqueFileName; // Trả về đường dẫn ảnh
        }

        #endregion

        #region Xóa sự kiện
        [Route("DeleteEvent")]
        [HttpPost]
        public IActionResult DeleteEvent(string id)
        {
            TempData["Message"] = "";

            var registrations = _db.Registrations.Where(x => x.EventId == id).ToList();
            var donations = _db.Donations.Where(x => x.EventId == id).ToList();

            if (registrations.Count > 0 || donations.Count > 0)
            {
                TempData["Message"] = "Can not delete";
                return RedirectToAction("Event");
            }

            var eventToDelete = _db.Events.Find(id);
            if (eventToDelete != null)
            {
                try
                {
                    // Logic xóa
                    _db.Events.Remove(_db.Events.Find(id));
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

        #region Danh sách người đăng kí tham gia
        [Route("GetRegisteredVolunteers")]
        public IActionResult GetRegisteredVolunteers(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            // Lấy danh sách đăng ký kèm theo thông tin Volunteer và Event
            var lstRegistered = _db.Registrations
                                   .Include(r => r.Volunteer) // Load dữ liệu Volunteer
                                   .Include(r => r.Event) // Load dữ liệu Event
                                   .AsNoTracking()
                                   .OrderBy(x => x.EventId)
                                   .ToPagedList(pageNumber, pageSize);

            return View(lstRegistered);
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

    }
}