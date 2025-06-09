using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;
using Volunteer_website.Models;

[Area("Organization")]
[Route("[area]/[controller]/[action]")]
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
        var colors = new List<string>
        {
            "#2b6cb0", "#e53e3e", "#48bb78", "#ed8936", "#9f7aea",
            "#f6ad55", "#38b2ac", "#805ad5", "#ecc94b", "#4299e1",
            "#d69e2e", "#276749", "#c53030", "#6b46c1", "#2d3748"
        };

        var eventsData = _db.Events
            .Select(e => new
            {
                title = e.Name ?? "Sự kiện không tên",
                start = e.DayBegin.HasValue ? e.DayBegin.Value.ToString("yyyy-MM-dd") : null,
                end = e.DayEnd.HasValue ? e.DayEnd.Value.AddDays(1).ToString("yyyy-MM-dd") : null,
                id = e.EventId, // EventId is a string
                description = e.Description ?? "Không có mô tả"
            })
            .ToList();

        var events = eventsData.Select((e, index) => new
        {
            title = e.title,
            start = e.start,
            end = e.end,
            id = e.id,
            color = colors[index % colors.Count],
            description = e.description
        });

        // Debug: Log events
        foreach (var eventItem in events)
        {
            Console.WriteLine($"Event ID: {eventItem.id}, Color: {eventItem.color}");
        }

        return Json(events);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateEventDate([FromBody] JsonElement request)
    {
        try
        {
            // Extract values from JsonElement
            string eventId = request.TryGetProperty("eventId", out JsonElement eventIdElement)
                ? eventIdElement.GetString()
                : null;
            string startDate = request.TryGetProperty("startDate", out JsonElement startDateElement)
                ? startDateElement.GetString()
                : null;
            string endDate = request.TryGetProperty("endDate", out JsonElement endDateElement)
                ? endDateElement.GetString()
                : null;

            // Debug: Log received values
            Console.WriteLine($"Received: eventId={eventId}, startDate={startDate}, endDate={endDate}");

            // Validate input
            if (string.IsNullOrEmpty(eventId) || string.IsNullOrEmpty(startDate))
            {
                TempData["ErrorMessage"] = "Thiếu thông tin bắt buộc (eventId hoặc startDate)";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            // Find the event (EventId is a string)
            var eventToUpdate = _db.Events.FirstOrDefault(e => e.EventId == eventId);
            if (eventToUpdate == null)
            {
                TempData["ErrorMessage"] = "Sự kiện không tồn tại";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            // Parse and validate start date
            if (!DateOnly.TryParse(startDate, out DateOnly parsedStartDate))
            {
                TempData["ErrorMessage"] = "Định dạng ngày bắt đầu không hợp lệ";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            // Parse end date if provided
            DateOnly? parsedEndDate = null;
            if (!string.IsNullOrEmpty(endDate))
            {
                if (!DateOnly.TryParse(endDate, out DateOnly tempEndDate))
                {
                    TempData["ErrorMessage"] = "Định dạng ngày kết thúc không hợp lệ";
                    return Json(new { success = false, message = TempData["ErrorMessage"] });
                }
                parsedEndDate = tempEndDate;

                // Validate that end date is not before start date
                if (parsedEndDate < parsedStartDate)
                {
                    TempData["ErrorMessage"] = "Ngày kết thúc phải sau ngày bắt đầu";
                    return Json(new { success = false, message = TempData["ErrorMessage"] });
                }
            }

            // Update the event
            eventToUpdate.DayBegin = parsedStartDate;
            eventToUpdate.DayEnd = parsedEndDate;

            // Save changes to the database
            _db.Events.Update(eventToUpdate);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật sự kiện thành công";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật sự kiện. Vui lòng thử lại.";
            return Json(new { success = false, message = TempData["ErrorMessage"] });
        }
    }
}