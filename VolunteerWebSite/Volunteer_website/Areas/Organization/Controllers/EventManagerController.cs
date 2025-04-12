using System.Data.Entity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Volunteer_website.Models;
using X.PagedList.Extensions;

namespace Volunteer_website.Areas.Organization.Controllers
{
    [Area("Organization")]
    [Route("[area]/[controller]/[action]")] // Sửa lại route template
    public class EventManagerController : Controller
    {

        private readonly VolunteerManagementContext _db;

        public EventManagerController(VolunteerManagementContext db)
        {
            _db = db;
        }


        #region Hiển thị sự kiện
       
        public IActionResult Index(int? page, string searchValue)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var query = _db.Events.AsNoTracking();
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(e => e.Name.Contains(searchValue)); 
            }

            var lstEvent = query.OrderBy(x => x.EventId)
                                .ToPagedList(pageNumber, pageSize);
            ViewBag.SearchValue = searchValue;

            return View(lstEvent);
        }

        #endregion

        #region Chỉnh sửa sự kiện
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
                existingEvent.ListImg = await SaveImageList(listImg);
            }

            _db.Update(existingEvent);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
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
        private async Task<string> SaveImageList(IFormFileCollection? listImages)
        {
            if (listImages == null || listImages.Count == 0)
            {
                return string.Empty;
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var savedPaths = new List<string>();

            foreach (var image in listImages)
            {
                if (image.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    savedPaths.Add("/uploads/" + uniqueFileName);
                }
            }

            return string.Join(",", savedPaths); // Trả về các đường dẫn cách nhau bởi dấu phẩy
        }
        #endregion


        #region Thêm sự kiện
        [HttpGet]
        public IActionResult Create()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
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

            
            if (listImg != null && listImg.Any())  
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
                return RedirectToAction("Index");
            }

            // Nếu có lỗi, hiển thị lại form
            return View(eventModel);
        }

        #endregion


        #region Xóa sự kiện
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            try
            {
                var eventToDelete = _db.Events.Find(id);
                if (eventToDelete == null)
                {
                    TempData["DeleteStatus"] = "error";
                    TempData["DeleteMessage"] = "Không tìm thấy sự kiện cần xóa";
                    return RedirectToAction("Index");
                }

                // Kiểm tra ràng buộc dữ liệu
                if (_db.Registrations.Any(x => x.EventId == id) || _db.Donations.Any(x => x.EventId == id))
                {
                    TempData["DeleteStatus"] = "error";
                    TempData["DeleteMessage"] = "Không thể xóa vì sự kiện đã có dữ liệu liên quan";
                    return RedirectToAction("Index");
                }

                _db.Events.Remove(eventToDelete);
                _db.SaveChanges();

                TempData["DeleteStatus"] = "success";
                TempData["DeleteMessage"] = "Xóa sự kiện thành công";
            }
            catch (Exception ex)
            {
                TempData["DeleteStatus"] = "error";
                TempData["DeleteMessage"] = $"Lỗi khi xóa sự kiện: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
        #endregion




        #region Xem chi tiết sự kiện
        [HttpGet]
        public IActionResult EventDetails(string id)
        {
            try
            {
                var eventDetail = _db.Events.FirstOrDefault(v => v.EventId == id);
                if (eventDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sự kiện." });
                }

                var registrationCount = _db.Registrations.Count(r => r.EventId == id);
                var donationCount = _db.Donations.Count(d => d.EventId == id);
                var totalAmount = _db.Donations.Where(d => d.EventId == id).Sum(d => d.Amount);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        eventDetail.EventId,
                        eventDetail.Name,
                        Type = eventDetail.type_event_name,
                        eventDetail.Location,
                        StartDate = eventDetail.DayBegin?.ToString("dd/MM/yyyy"),
                        EndDate = eventDetail.DayEnd?.ToString("dd/MM/yyyy"),
                        eventDetail.TargetMember,
                        eventDetail.TargetFunds,
                        eventDetail.Description,
                        eventDetail.Status,
                        eventDetail.ImagePath,
                        RegistrationCount = registrationCount,
                        DonationCount = donationCount,
                        TotalAmount = totalAmount
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra.", error = ex.Message });
            }
        }
        #endregion

    }
}
