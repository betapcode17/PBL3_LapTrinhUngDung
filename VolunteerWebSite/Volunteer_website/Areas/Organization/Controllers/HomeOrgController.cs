
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Data;
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
                            .OrderBy(x => x.Name)
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
    }
    #endregion
    #region Chỉnh sửa sự kiện
    #endregion
}