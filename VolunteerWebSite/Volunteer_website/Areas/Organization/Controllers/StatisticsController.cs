using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volunteer_website.Models;

namespace Volunteer_website.Areas.Organization.Controllers
{
    [Area("Organization")]
    [Route("Organization/[controller]/[action]")]
    [Authorize("Org")]
    public class StatisticsController : Controller
    {
        private readonly VolunteerManagementContext _db;

        public StatisticsController(VolunteerManagementContext db)
        {
            _db = db;
        }

        #region Thống kê tổng quát
        public IActionResult Index()
        {
            var orgId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgId))
            {
                return RedirectToAction("Login", "Account"); // hoặc xử lý tùy ý
            }

            var eventIds = _db.Events
                         .Where(e => e.OrgId == orgId)
                         .Select(e => e.EventId)
                         .ToList();

            ViewBag.TotalVolunteers = _db.Registrations
                .Where(r => eventIds.Contains(r.EventId))
                .Select(r => r.VolunteerId)
                .Distinct()
                .Count();

            ViewBag.TotalEvents = _db.Events
                .Count(e => e.OrgId == orgId);

            ViewBag.TotalRegistrations = _db.Registrations
      .Count(r => _db.Events
          .Where(e => e.OrgId == orgId)
          .Select(e => e.EventId)
          .Contains(r.EventId));

            ViewBag.UncompletedEvaluations = _db.Evaluations
                .Count(e => !e.IsCompleted &&
                    _db.Events
                        .Where(ev => ev.OrgId == orgId)
                        .Select(ev => ev.EventId)
                        .Contains(e.Reg.EventId));

            return View();
        }
        #endregion

        #region Số lượt đăng kí theo tháng
        public IActionResult GetRegistrationByMonth()
        {
            var orgId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var eventIds = _db.Events
                              .Where(e => e.OrgId == orgId)
                              .Select(e => e.EventId)
                              .ToList();

           
            var data = _db.Registrations
                          .Where(r => eventIds.Contains(r.EventId) && r.RegisterAt != null)
                          .GroupBy(r => new { r.RegisterAt.Value.Year, r.RegisterAt.Value.Month })
                          .Select(g => new
                          {
                              Year = g.Key.Year,
                              Month = g.Key.Month,
                              Count = g.Count()
                          })
                          .OrderBy(x => x.Year).ThenBy(x => x.Month)
                          .ToList();

           
            return Json(data);
        }
        #endregion


        #region Số lượng sự kiện theo tháng
        [HttpGet]
        public IActionResult GetEventsByMonth()
        {
            var orgId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var data = _db.Events
                .Where(e => e.OrgId == orgId && e.DayBegin != null)
                .GroupBy(e => new { e.DayBegin.Value.Year, e.DayBegin.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToList();

            return Json(data);
        }
        #endregion



        #region Số lượt quyên góp theo tháng 
        [HttpGet]
        public IActionResult GetDonationByMonth()
        {
            var orgID = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(orgID))
            {
                return Unauthorized(); // Không có orgID => không được truy cập
            }

            var data = _db.Donations
                .Where(d => d.DonationDate != null && d.Event != null && d.Event.OrgId == orgID)
                .GroupBy(d => new { d.DonationDate.Value.Year, d.DonationDate.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToList();

            return Json(data);
        }
        #endregion

        #region Tổng số tiền quyên góp theo tháng
        [HttpGet]
        public IActionResult GetDonationAmountByMonth()
        {
            var orgID = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(orgID))
            {
                return Unauthorized(); // Không có orgID => không được truy cập
            }

            var data = _db.Donations
                .Where(d => d.DonationDate != null && d.Event != null && d.Event.OrgId == orgID)
                .GroupBy(d => new { d.DonationDate.Value.Year, d.DonationDate.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(d => d.Amount)
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToList();

            return Json(data);
        }
        #endregion

    }
}
