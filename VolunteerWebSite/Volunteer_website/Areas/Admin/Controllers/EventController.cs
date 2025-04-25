using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Areas.Admin.Data;
using Volunteer_website.Models;
using X.PagedList.Extensions;

namespace Volunteer_website.Areas.Admin.Controllers
{

    public class EventController : Controller
    {
        private readonly VolunteerManagementContext _db;
        public EventController(VolunteerManagementContext context) 
        {
            _db = context;
        }

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
    }
}
