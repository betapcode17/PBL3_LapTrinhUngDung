using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Volunteer_website.Models;
using X.PagedList.Extensions;
using Volunteer_website.Helpers;

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
                query = query.Where(e => e.Name != null && e.Name.Contains(searchValue)); 
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
            ViewBag.OrgId = eventModel.OrgId; 
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
            existingEvent.Name = model.Name;
            existingEvent.TargetMember = model.TargetMember;
            existingEvent.Status = model.Status;
            existingEvent.TypeEventName = model.TypeEventName;
            existingEvent.DayBegin = model.DayBegin;
            existingEvent.DayEnd = model.DayEnd;
            existingEvent.Location = model.Location;
            existingEvent.TargetFunds = model.TargetFunds;
            existingEvent.Description = model.Description;          
            if (imagePath != null)
            {
                existingEvent.ImagePath = await UpLoadImgService.UploadImg(imagePath, "EventsImg");
            }        
            if (listImg != null && listImg.Any())
            {
                var uploadedImages = await UpLoadImgService.UploadListImg(listImg, "EventsImg");
                existingEvent.ListImg = string.Join(",", uploadedImages); 
            }

            _db.Update(existingEvent);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thay đổi sự kiện thành công";
            return RedirectToAction("Index");
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

         
            var lastEvent = _db.Events
                .OrderByDescending(e => e.EventId)
                .FirstOrDefault();

            string newEventId = "EVT1";
            if (lastEvent != null)
            {
                // Giả sử EventId có định dạng "EVT" + số (EVT1, EVT2, ...)
                string lastIdNumber = lastEvent.EventId.Replace("EVT", "");
                if (int.TryParse(lastIdNumber, out int number))
                {
                    newEventId = $"EVT{number + 1}";
                }
            }         
            var model = new Event
            {
                EventId = newEventId,
                OrgId = orgId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventModel,IFormFile imagePath,IFormFileCollection? listImg)  
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(eventModel);
            }

            if (eventModel != null)
            {
               

                if (imagePath != null)
                {
                    eventModel.ImagePath = await UpLoadImgService.UploadImg(imagePath, "EventsImg");
                }



                if (listImg != null && listImg.Any())
                {
                    var uploadedImages = await UpLoadImgService.UploadListImg(listImg, "EventsImg");
                    eventModel.ListImg = string.Join(",", uploadedImages);
                }
                if (ModelState.IsValid)
                {
                    eventModel.Status = "PENDING";
                    _db.Add(eventModel);
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm sự kiện thành công";
                    return RedirectToAction("Index");
                }
            }
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
                    TempData["Error"] = "error";
                    TempData["Error"] = "Không tìm thấy sự kiện cần xóa";
                    return RedirectToAction("Index");
                }

               
                if (_db.Registrations.Any(x => x.EventId == id) || _db.Donations.Any(x => x.EventId == id))
                {
                    TempData["Error"] = "error";
                    TempData["Error"] = "Không thể xóa vì sự kiện đã có dữ liệu liên quan";
                    return RedirectToAction("Index");
                }

                _db.Events.Remove(eventToDelete);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "success";
                TempData["SuccessMessage"] = "Xóa sự kiện thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "error";
                TempData["Error"] = $"Lỗi khi xóa sự kiện: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
        #endregion



        #region Xem chi tiet su kien
        [HttpGet]
        public async Task<IActionResult> EventDetails(string id)
        {
            var orgId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Lấy chi tiết sự kiện
            var eventDetail = await _db.Events
                                       .Where(x => x.EventId == id && x.OrgId == orgId)
                                       .FirstOrDefaultAsync();

            if (eventDetail == null) return NotFound();

            
            var registrations = _db.Registrations
                                    .Where(r => r.EventId == id)
                                    .OrderByDescending(r => r.RegisterAt)
                                    .Take(10)
                                    .ToList();

          
            ViewBag.Participants = registrations
         .Select(r => new
         {
             Name = _db.Volunteers.FirstOrDefault(v => v.VolunteerId == r.VolunteerId)?.Name ?? "Unknown",
             Time = r.RegisterAt.HasValue
                     ? r.RegisterAt.Value.ToString("dd/MM/yyyy") // Adjusted format for DateOnly
                     : "N/A"
         }).ToList();

            return View(eventDetail);
        }
        #endregion



    }
}
