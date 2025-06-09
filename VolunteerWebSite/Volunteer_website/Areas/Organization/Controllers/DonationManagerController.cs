//
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Volunteer_website.Models;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Data.Entity;

namespace Volunteer_website.Areas.Organizations.Controllers
{
    [Area("Organization")]
    [Route("[area]/[controller]/[action]")] // Sửa lại route template
    [Authorize("Org")]
    public class DonationManagerController : Controller
    {
        private readonly VolunteerManagementContext _db;

        public DonationManagerController(VolunteerManagementContext db)
        {
            _db = db;
        }

        public IActionResult Index(int? page, string searchValue)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            // Explicitly specify the namespace to resolve ambiguity  
            var query = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(_db.Donations);

            if (!string.IsNullOrEmpty(searchValue))
            {
                var matchingEventIds = _db.Events
                                          .Where(e => e.Name.Contains(searchValue))
                                          .Select(e => e.EventId)
                                          .ToList();

                var matchingVolunteerIds = _db.Volunteers
                                              .Where(v => v.Name.Contains(searchValue))
                                              .Select(v => v.VolunteerId)
                                              .ToList();

                query = query.Where(d =>
                    matchingEventIds.Contains(d.EventId) ||
                    matchingVolunteerIds.Contains(d.VolunteerId));
            }

            var lstDonated = query.OrderBy(x => x.DonationId)
                                  .ToPagedList(pageNumber, pageSize);

            var volunteerIds = lstDonated.Select(d => d.VolunteerId).Distinct().ToList();
            var volunteers = _db.Volunteers
                                .Where(v => volunteerIds.Contains(v.VolunteerId))
                                .ToDictionary(v => v.VolunteerId, v => v);

            var eventIds = lstDonated.Select(d => d.EventId).Distinct().ToList();
            var events = _db.Events
                            .Where(e => eventIds.Contains(e.EventId))
                            .ToDictionary(e => e.EventId, e => e);

            ViewBag.Volunteers = volunteers;
            ViewBag.Events = events;
            ViewBag.SearchValue = searchValue;

            return View(lstDonated);
        }

        [HttpGet]
        public IActionResult GetVolunteerDetails(string id)
        {
            var volunteer = _db.Volunteers.FirstOrDefault(v => v.VolunteerId == id);
            if (volunteer == null) return NotFound();

            var result = new
            {
                volunteerId = volunteer.VolunteerId,
                name = volunteer.Name,
                email = volunteer.Email,
                phoneNumber = volunteer.PhoneNumber,
                dateOfBirth = volunteer.DateOfBirth?.ToString("yyyy-MM-dd"),
                gender = volunteer.Gender,
                address = volunteer.Address,
                imagePath = volunteer.ImagePath
            };
            return Json(result);
        }
    }
}
//