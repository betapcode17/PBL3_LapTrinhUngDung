using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Volunteer_website.ViewModels;
using Volunteer_website.Models;
using Volunteer_website.Services;
namespace Volunteer_website.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VolunteerManagementContext _context;
        private readonly IVnPayService _vnPayService;
        public HomeController(ILogger<HomeController> logger, VolunteerManagementContext context, IVnPayService vnPayService)
        {
            _logger = logger;
            _context = context;
            _vnPayService = vnPayService;
        }

        public IActionResult Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Now); // 04:19 PM +07, May 27, 2025

            // 1. Lấy dữ liệu cho "Our Causes" (các sự kiện có TargetFunds > 0)
            var causes = _context.Events
                .Include(e => e.Donations)
                .Where(e => e.TargetFunds > 0)
                .OrderByDescending(e => e.DayBegin)
                .Take(6)
                .ToList();

            ViewBag.Causes = causes ?? new List<Event>();

            // 2. Lấy dữ liệu cho "Our Latest Events" (sự kiện sắp diễn ra, 3 sự kiện mới nhất)
            var latestEvents = _context.Events
                .Where(e => e.DayBegin > today)
                .OrderBy(e => e.DayBegin)
                .Take(3)
                .ToList();

            ViewBag.LatestEvents = latestEvents ?? new List<Event>();

            // 3. Lấy dữ liệu cho "Latest Donations" (kết hợp Donations và Registrations)
            var latestDonations = _context.Donations
               .Include(d => d.Volunteer)
               .Include(d => d.Event)
               .Select(d => new
               {
                   Type = "Donation",
                   Name = d.Volunteer != null ? d.Volunteer.Name ?? "Ẩn danh" : "Ẩn danh",
                   Amount = d.Amount,
                   EventName = d.Event != null ? d.Event.Name ?? "Không xác định" : "Không xác định",
                   ActionTime = d.DonationDate ?? DateTime.Now,
                   ImagePath = d.Volunteer != null && !string.IsNullOrEmpty(d.Volunteer.ImagePath)
                       ? d.Volunteer.ImagePath
                       : "~/images/DefaultImg/default-person.jpg"
               })
               .ToList();

            var latestRegistrations = _context.Registrations
                           .Include(r => r.Volunteer)
                           .Include(r => r.Event)
                           .Where(r => r.Status == "Được duyệt")
                           .Select(r => new
                           {
                               Type = "Volunteer",
                               Name = r.Volunteer != null ? r.Volunteer.Name ?? "Ẩn danh" : "Ẩn danh",
                               Amount = (decimal?)null,
                               EventName = r.Event != null ? r.Event.Name ?? "Không xác định" : "Không xác định",
                               ActionTime = r.RegisterAt != null ? r.RegisterAt.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.Now,
                               ImagePath = r.Volunteer != null && !string.IsNullOrEmpty(r.Volunteer.ImagePath)
                                   ? r.Volunteer.ImagePath
                                   : "~/images/default-person.jpg"
                           })
                           .ToList();

            // Combine and cast to a common type
            var latestActions = latestDonations
                .Cast<object>()
                .Concat(latestRegistrations.Cast<object>())
                .OrderByDescending(a => ((dynamic)a).ActionTime)
                .Take(3)
                .ToList();

            // Assign to ViewBag
            ViewBag.LatestDonations = latestActions.Any() ? latestActions : new List<object>();

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Events(int page = 1, int pageSize = 6, string statusFilter = "ongoing", string eventName = "", string organization = "", string startDateFrom = "", string startDateTo = "", string eventType = "")
        {
            var query = _context.Events.AsQueryable();

            // Áp dụng các bộ lọc
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "ongoing")
                {
                    query = query.Where(e => e.DayBegin <= today && e.DayEnd >= today);
                }
                else if (statusFilter == "upcoming")
                {
                    query = query.Where(e => e.DayBegin > today);
                }
                else if (statusFilter == "ended")
                {
                    query = query.Where(e => e.DayEnd < today);
                }
            }

            if (!string.IsNullOrEmpty(eventName))
            {
                query = query.Where(e => e.Name != null && e.Name.Contains(eventName));
            }

            if (!string.IsNullOrEmpty(organization))
            {
                query = query.Where(e => e.Org != null && e.Org.Name != null && e.Org.Name.Contains(organization));
            }

            if (!string.IsNullOrEmpty(startDateFrom))
            {
                var startDate = DateOnly.Parse(startDateFrom);
                query = query.Where(e => e.DayBegin >= startDate || e.DayEnd >= startDate);
            }

            if (!string.IsNullOrEmpty(startDateTo))
            {
                var endDate = DateOnly.Parse(startDateTo);
                query = query.Where(e => e.DayBegin <= endDate || e.DayEnd <= endDate);
            }

            if (!string.IsNullOrEmpty(eventType))
            {
                query = query.Where(e => e.TypeEvent != null && e.TypeEvent.Name == eventType);
            }

            // Tính tổng số sự kiện sau khi lọc
            var totalEvents = query.Count();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalEvents / pageSize);

            // Đảm bảo số trang hợp lệ
            page = Math.Max(1, page);
            page = Math.Min(page, totalPages > 0 ? totalPages : 1);

            // Lấy danh sách sự kiện đã phân trang
            var eventList = query
                .OrderByDescending(e => e.DayBegin)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventModel
                {
                    Event_Id = e.EventId,
                    Name = e.Name ?? "Không có tên",
                    EventDescription = e.Description ?? "Không có mô tả",
                    ImagePath = e.ImagePath ?? "/images/default-event.jpg",
                    DateBegin = e.DayBegin ?? DateOnly.FromDateTime(DateTime.Today),
                    DateEnd = e.DayEnd ?? DateOnly.FromDateTime(DateTime.Today),
                    Location = e.Location ?? "N/A",
                    Organization = e.Org != null ? e.Org.Name ?? "N/A" : "N/A",
                    targetmember = e.TargetMember ?? 0,
                    targetfund = e.TargetFunds.HasValue ? (int)e.TargetFunds.Value : 0,
                    currentmember = e.Registrations.Count(ev => ev.Status == "Được duyệt"),
                    currentfund = e.Donations != null ? (int)e.Donations.Sum(d => d.Amount ?? 0) : 0,
                    type = e.TypeEvent != null ? e.TypeEvent.Name ?? "N/A" : "N/A"
                })
                .ToList();

            // Truyền thông tin phân trang và bộ lọc cho view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.EventName = eventName;
            ViewBag.Organization = organization;
            ViewBag.StartDateFrom = startDateFrom;
            ViewBag.StartDateTo = startDateTo;
            ViewBag.EventType = eventType;

            return View(eventList);
        }

        [HttpGet]
        public IActionResult Detail_Event(string id)
        {
            var eventDetail = _context.Events.FirstOrDefault(e => e.EventId == id);
            if (eventDetail == null)
            {
                return NotFound();
            }

            var event_detail = (from v in _context.Events
                                where v.EventId == id
                                join ev in _context.Organizations
                                on v.OrgId equals ev.OrgId
                                //join e in _context.EventImages
                                //on v.EventId equals e.EventId
                                join d in _context.Donations
                                on v.EventId equals d.EventId into donations
                                select new EventModel
                                {
                                    Event_Id = v.EventId,
                                    Name = v.Name,
                                    EventDescription = v.Description,
                                    ImagePath = v.ImagePath,
                                    DateBegin = v.DayBegin ?? DateOnly.FromDateTime(DateTime.Today),
                                    DateEnd = v.DayEnd ?? DateOnly.FromDateTime(DateTime.Today),
                                    Location = v.Location,
                                    Organization = ev.Name,
                                    OrganizationImagePath = ev.ImagePath,
                                    OrganizationDescription = ev.Description,
                                    OrganizationPhone = ev.PhoneNumber,
                                    OrganizationEmail = ev.Email,
                                    targetmember = v.TargetMember ?? 0,
                                    targetfund = v.TargetFunds.HasValue ? (int)v.TargetFunds.Value : 0,
                                    currentmember = v.Donations.Count(d=>d.EventId==id),
                                    currentfund = v.Donations != null ? (int)v.Donations.Sum(d => d.Amount ?? 0) : 0
                                    
                                }).FirstOrDefault();
            var donationDetails = (from v in _context.Volunteers
                                    join d in _context.Donations on v.VolunteerId equals d.VolunteerId
                                    where d.EventId == id
                                    orderby d.DonationDate descending 
                                    select new Donate_List
                                    {
                                        VolunteerName = v.Name ?? "Unknown",
                                        Volunteer_Id = v.VolunteerId,
                                        EventId = d.EventId,
                                        Amount = d.Amount,
                                        DonationDate = d.DonationDate
                                    }).ToList();

            if (event_detail != null)
            {
                event_detail.Donations = donationDetails;
            }

            return View(event_detail);
        }
        


        public IActionResult Volunteers()
        {
            return View();
        }
        public IActionResult Blogs()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Volunteer()
        {
            return View();
        }
        public IActionResult IndexUser()
        {
            return View();
        }
        public IActionResult Volunteer_List()
        {
            var volunteers = _context.Volunteers
                .Select(v => new Volunteer_List
                {
                    VolunteerId = v.VolunteerId,
                    Name = v.Name,
                    Email = v.Email,
                    PhoneNumber = v.PhoneNumber,
                    JoinDate = null // Vì không có thông tin đăng ký, đặt null hoặc loại bỏ nếu không cần
                })
                .ToList();

            return View(volunteers);
        }

    }
}
