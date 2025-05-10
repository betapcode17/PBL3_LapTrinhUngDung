using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Areas.Admin.Data;
using Volunteer_website.Models;

namespace Volunteer_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Users")]
    public class UsersController : Controller
    {
        private readonly VolunteerManagementContext _context;

        public UsersController(VolunteerManagementContext context)
        {
            _context = context;
        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        #region Ban and Unban user
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("BanUser")]
        public async Task<IActionResult> BanUser([FromBody] UserRequest request)
        {
            try
            {
                var VolunteerId = request.UserId;
                var existingUser = _context.Users.FirstOrDefault(ev => ev.UserId == VolunteerId);
                if (existingUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // chuyển trạng thái status
                existingUser.IsActive = false;

                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User Unban successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("UnBanUser")]
        public async Task<IActionResult> UnBanUser([FromBody] UserRequest request)
        {
            try
            {
                var VolunteerId = request.UserId;
                var existingUser = _context.Users.FirstOrDefault(ev => ev.UserId == VolunteerId);
                if (existingUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // chuyển trạng thái status
                existingUser.IsActive = true;

                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User Unban successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion
    }
}
