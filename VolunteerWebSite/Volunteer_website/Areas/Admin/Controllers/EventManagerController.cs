using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Areas.Admin.Data;
using Volunteer_website.Models;
using X.PagedList.Extensions;

namespace Volunteer_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("Admin")]
    [Route("Admin/EventManager")] 
    public class EventManagerController : Controller
    {
        private readonly VolunteerManagementContext _db;
        public EventManagerController(VolunteerManagementContext context) 
        {
            _db = context;
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

        #region lấy dữ liệu event
        [Route("GetEventDetails")]
        public async Task<IActionResult> GetEventDetails(string id)
        {
            var eventObj = await _db.Events
                .Include(e => e.Org)  // Include related organization
                .FirstOrDefaultAsync(ev => ev.EventId == id);

            if (eventObj == null)
                return Json(new { success = false, message = "Event not found" });

            // Count registrations
            var registrationCount = await _db.Registrations.CountAsync(r => r.EventId == id);

            // Get donations information
            var donations = await _db.Donations
                .Where(d => d.EventId == id)
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = new
                {
                    eventId = eventObj.EventId,
                    name = eventObj.Name,
                    description = eventObj.Description,
                    location = eventObj.Location,
                    dayBegin = eventObj.DayBegin,
                    dayEnd = eventObj.DayEnd,
                    targetMember = eventObj.TargetMember,
                    targetFunds = eventObj.TargetFunds,
                    type_event_name = eventObj.TypeEventId,
                    organizationName = eventObj.Org?.Name,
                    status = eventObj.Status,
                    imagePath = eventObj.ImagePath,
                    listImg = eventObj.ListImg,
                    registrationCount = registrationCount,
                    donationCount = donations.Count,
                    totalAmount = donations.Sum(d => d.Amount ?? 0)
                }
            });
        }
        #endregion


        #region Danh Sach Doantion
        [Route("ListVolunteerDonation")]
        public IActionResult ListVolunteerDonation(string id, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var ListVolunteerDonate = _db.Donations
                .Where(r => r.EventId == id)
                .ToPagedList(pageNumber, pageSize);
            return View(ListVolunteerDonate);
        }
        #endregion

        #region Danh sách volunteer
        [Route("ListVolunteerRegistrationm")]
        public IActionResult ListVolunteerRegistration(string id, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var ListVolunteerRegis = _db.Registrations
                .Where(r => r.EventId == id)
                .ToPagedList(pageNumber, pageSize);
            return View(ListVolunteerRegis);
        }
        #endregion
    }
}
