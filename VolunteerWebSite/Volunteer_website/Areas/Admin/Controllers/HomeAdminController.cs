using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volunteer_website.Models;
using Volunteer_website.Areas.Admin.Data;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Azure.Core;
namespace Volunteer_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/HomeAdmin")]
    public class HomeAdminController : Controller
    {
        private readonly VolunteerManagementContext _db;

        public HomeAdminController(VolunteerManagementContext db)
        {
            _db = db;
        }

        #region Trang chủ
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            var events = _db.Events.Count();
            var totalAmount = _db.Donations.AsNoTracking()
                                   .Sum(d => d.Amount);
            var volunteers = _db.Volunteers.Count();

            var currentYear = DateTime.Now.Year;
            var startYear = currentYear - 5;
            int[] eventsByYear = new int[6];

            for (int i = 0; i < 6; i++)
            {
                var year = startYear + i;
                eventsByYear[i] = _db.Events.Count(e => e.DayBegin.HasValue && e.DayBegin.Value.Year == year);
            }

            ViewBag.Events = events;
            ViewBag.Donations = totalAmount;
            ViewBag.Volunteers = volunteers;
            ViewBag.EventsByYear = eventsByYear;

            return View();
        }
        #endregion

        #region Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account", new { area = "" });
        }
        #endregion
    }
}
