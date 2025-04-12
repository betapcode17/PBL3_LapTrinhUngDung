using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volunteer_website.Models;
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
        #region Trang chủ
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
        #endregion

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
    }
}