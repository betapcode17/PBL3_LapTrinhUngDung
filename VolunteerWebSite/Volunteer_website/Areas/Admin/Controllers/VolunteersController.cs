using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Models;
using X.PagedList.Extensions;

namespace Volunteer_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("Admin")]
    public class VolunteersController : Controller
    {
        private readonly VolunteerManagementContext _context;

        public VolunteersController(VolunteerManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 8;
            int PageNumber = page;

            var listVolunteers =  _context.Volunteers
                .OrderBy(v => v.VolunteerId)
                .ToPagedList(PageNumber, pageSize);
            return View(listVolunteers);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(m => m.VolunteerId == id);
            if (volunteer == null)
            {
                return NotFound();
            }

            return View(volunteer);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VolunteerId,PhoneNumber,Email,Name,DateOfBirth,Gender,ImagePath,Address")] Volunteer volunteer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(volunteer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(volunteer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteer = await _context.Volunteers.FindAsync(id);
            if (volunteer == null)
            {
                return NotFound();
            }
            return View(volunteer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("VolunteerId,PhoneNumber,Email,Name,DateOfBirth,Gender,ImagePath,Address")] Volunteer volunteer)
        {
            if (id != volunteer.VolunteerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(volunteer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VolunteerExists(volunteer.VolunteerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(volunteer);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(m => m.VolunteerId == id);
            if (volunteer == null)
            {
                return NotFound();
            }

            return View(volunteer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var volunteer = await _context.Volunteers.FindAsync(id);
            if (volunteer != null)
            {
                _context.Volunteers.Remove(volunteer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VolunteerExists(string id)
        {
            return _context.Volunteers.Any(e => e.VolunteerId == id);
        }
    }
}
