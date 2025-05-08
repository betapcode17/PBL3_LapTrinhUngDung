using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Volunteer_website.Models;
[Area("Organization")]
[Route("[area]/[controller]/[action]")] // Sửa lại route template
[Authorize("Org")]
public class CalendarController : Controller
{
    private readonly VolunteerManagementContext _db;

    public CalendarController(VolunteerManagementContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetEventsCalendar()
    {
      
        var today = DateOnly.FromDateTime(DateTime.Now);
        var events = _db.Events
            .Where(e => e.DayBegin.HasValue && e.DayBegin.Value >= today)
            .Select(e => new
            {
                title = e.Name ?? "Sự kiện không tên",
                start = e.DayBegin.HasValue ? e.DayBegin.Value.ToString("yyyy-MM-dd") : null,
                end = e.DayEnd.HasValue ? e.DayEnd.Value.AddDays(1).ToString("yyyy-MM-dd") : null, 
                id = e.EventId,
                color = "#2b6cb0", 
                description = e.Description ?? "Không có mô tả" 
            })
            .ToList();

        return Json(events);
    }

  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateEventDate(string eventId, string startDate, string endDate)
    {
        try
        {
            // Validate input
            if (string.IsNullOrEmpty(eventId) || string.IsNullOrEmpty(startDate))
            {
                return Json(new { success = false, message = "Thiếu thông tin bắt buộc (eventId hoặc startDate)" });
            }

            // Find the event
            var eventToUpdate = _db.Events.FirstOrDefault(e => e.EventId == eventId); // Assuming Id is the property name
            if (eventToUpdate == null)
            {
                return Json(new { success = false, message = "Sự kiện không tồn tại" });
            }

            // Parse and validate start date
            if (!DateOnly.TryParse(startDate, out DateOnly parsedStartDate))
            {
                return Json(new { success = false, message = "Định dạng ngày bắt đầu không hợp lệ" });
            }

            // Parse end date if provided
            DateOnly? parsedEndDate = null;
            if (!string.IsNullOrEmpty(endDate))
            {
                if (!DateOnly.TryParse(endDate, out DateOnly tempEndDate))
                {
                    return Json(new { success = false, message = "Định dạng ngày kết thúc không hợp lệ" });
                }
                parsedEndDate = tempEndDate;

                // Validate that end date is not before start date
                if (parsedEndDate < parsedStartDate)
                {
                    return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu" });
                }
            }

            // Update the event
            eventToUpdate.DayBegin = parsedStartDate;
            eventToUpdate.DayEnd = parsedEndDate; // This assumes DayEnd is nullable DateOnly?

            // Save changes to the database
            _db.Events.Update(eventToUpdate); // Use Update method directly
            _db.SaveChanges();

            return Json(new { success = true, message = "Cập nhật sự kiện thành công" });
        }
        catch (Exception ex)
        {
            // Log the exception (e.g., using a logging framework like Serilog or ILogger)
            // _logger.LogError(ex, "Error updating event with ID: {EventId}", eventId);
            return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật sự kiện. Vui lòng thử lại." });
        }
    }
}