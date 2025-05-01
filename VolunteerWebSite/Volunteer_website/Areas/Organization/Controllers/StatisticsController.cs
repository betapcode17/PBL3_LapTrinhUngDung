using System;
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






        [HttpGet]
        public IActionResult Index(string searchValue = "", string? startDate = null, string? endDate = null)
        {
            var orgId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgId))
            {
                return RedirectToAction("Login", "Account");
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
                .Count(r => eventIds.Contains(r.EventId));

            ViewBag.UncompletedEvaluations = _db.Evaluations
                .Count(e => !e.IsCompleted && eventIds.Contains(e.Reg.EventId));

            // Xử lý ngày mặc định nếu startDate hoặc endDate là null
            DateOnly startDateOnly, endDateOnly;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                startDateOnly = DateOnly.Parse(startDate);
                endDateOnly = DateOnly.Parse(endDate);
            }
            else
            {
                var endDateTime = DateTime.Now;
                var startDateTime = endDateTime.AddDays(-30);
                startDateOnly = DateOnly.FromDateTime(startDateTime);
                endDateOnly = DateOnly.FromDateTime(endDateTime);
            }

            // Định dạng ngày thành chuỗi "yyyy-MM-dd" để hiển thị trong input
            ViewBag.StartDate = startDateOnly.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDateOnly.ToString("yyyy-MM-dd");

            // Lấy dữ liệu thống kê đăng ký
            var statisticsData = StatisticsRegistrationByTime(searchValue, startDate, endDate);
            var registrationChartData = statisticsData.Select(x => new
            {
                Label = $"{x.Year}-{x.Month:D2}-{x.Day:D2}", // Bao gồm cả ngày
                Count = x.Count
            }).ToList();

            ViewBag.RegistrationStatistics = new
            {
                Labels = registrationChartData.Select(x => x.Label).ToArray(),
                Data = registrationChartData.Select(x => x.Count).ToArray()
            };

            // Lấy dữ liệu thống kê donation
            var donationStatistics = StatisticsDonationByTime(searchValue, startDate, endDate);
            var donationChartData = donationStatistics.Select(x => new
            {
                Label = $"{x.Year}-{x.Month:D2}-{x.Day:D2}", // Bao gồm cả ngày
                TotalAmount = x.TotalAmount
            }).ToList();

            ViewBag.DonationStatistics = new
            {
                Labels = donationChartData.Select(x => x.Label).ToArray(),
                Data = donationChartData.Select(x => x.TotalAmount).ToArray()
            };

            ViewBag.SearchValue = searchValue;

            return View();
        }


        #region Thống kê số lượt đăng kí theo thời gian
        public List<dynamic> StatisticsRegistrationByTime(string searchValue = "", string? startDate = null, string? endDate = null)
        {
            DateOnly startDateOnly, endDateOnly;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                startDateOnly = DateOnly.Parse(startDate);
                endDateOnly = DateOnly.Parse(endDate);
            }
            else
            {
                var endDateTime = DateTime.Now;
                var startDateTime = endDateTime.AddDays(-30);
                startDateOnly = DateOnly.FromDateTime(startDateTime);
                endDateOnly = DateOnly.FromDateTime(endDateTime);
            }

            var orgId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgId))
            {
                Console.WriteLine("No orgId found, returning empty list.");
                return new List<dynamic>();
            }

            var eventQuery = _db.Events.Where(e => e.OrgId == orgId);
            if (!string.IsNullOrEmpty(searchValue))
            {
                eventQuery = eventQuery.Where(e => e.Name != null && e.Name.Contains(searchValue));
            }

            var eventIds = eventQuery.Select(e => e.EventId).ToList();
            Console.WriteLine($"Event IDs for Registrations: {string.Join(", ", eventIds)}");

            var data = _db.Registrations
                .Where(r => eventIds.Contains(r.EventId) && r.RegisterAt.HasValue)
                .AsEnumerable() // Chuyển sang client-side để xử lý DateOnly
                .Where(r =>
                {
                    // Fixing the CS1503 error by converting DateOnly to DateTime before using it  
                    var registerDate = r.RegisterAt!.Value.ToDateTime(TimeOnly.MinValue);
                    // Convert DateOnly to DateTime for comparison
                    var startDateTime = startDateOnly.ToDateTime(TimeOnly.MinValue);
                    var endDateTime = endDateOnly.ToDateTime(TimeOnly.MinValue);
                   

                    return registerDate >= startDateTime && registerDate <= endDateTime;
                })
                .GroupBy(r => new
                {
                    Year = r.RegisterAt!.Value.Year,
                    Month = r.RegisterAt.Value.Month,
                    Day = r.RegisterAt.Value.Day // Thêm Day vào GroupBy
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Day = g.Key.Day, // Thêm Day vào kết quả
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.Day)
                .ToList();

            Console.WriteLine($"Registration Statistics Data: {Newtonsoft.Json.JsonConvert.SerializeObject(data)}");

            return data.Select(x => (dynamic)x).ToList();
        }
        #endregion


        #region Thống kê số tiền Donation theo thời gian
        public List<dynamic> StatisticsDonationByTime(string searchValue = "", string? startDate = null, string? endDate = null)
        {
            DateOnly startDateOnly, endDateOnly;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                startDateOnly = DateOnly.Parse(startDate);
                endDateOnly = DateOnly.Parse(endDate);
            }
            else
            {
                var endDateTime = DateTime.Now;
                var startDateTime = endDateTime.AddDays(-30);
                startDateOnly = DateOnly.FromDateTime(startDateTime);
                endDateOnly = DateOnly.FromDateTime(endDateTime);
            }

            var orgId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgId))
            {
                Console.WriteLine("No orgId found, returning empty list.");
                return new List<dynamic>();
            }

            var eventQuery = _db.Events.Where(e => e.OrgId == orgId);
            if (!string.IsNullOrEmpty(searchValue))
            {
                eventQuery = eventQuery.Where(e => e.Name != null && e.Name.Contains(searchValue));
            }

            var eventIds = eventQuery.Select(e => e.EventId).ToList();
            Console.WriteLine($"Event IDs for Donations: {string.Join(", ", eventIds)}");

            var data = _db.Donations
                .Where(r => eventIds.Contains(r.EventId) && r.DonationDate.HasValue)
                .AsEnumerable() // Chuyển sang client-side để xử lý DateOnly
                .Where(r =>
                {
                    // Fixing the CS1503 error by converting DateOnly to DateTime before using it
                    var donationDate = r.DonationDate!.Value.ToDateTime(TimeOnly.MinValue);
                  
                    // Fixing the CS0019 errors by converting DateOnly to DateTime before comparison
                    var startDateTime = startDateOnly.ToDateTime(TimeOnly.MinValue);
                    var endDateTime = endDateOnly.ToDateTime(TimeOnly.MinValue);
                    return donationDate >= startDateTime && donationDate <= endDateTime;
                })
                .GroupBy(r => new
                {
                    Year = r.DonationDate!.Value.Year,
                    Month = r.DonationDate.Value.Month,
                    Day = r.DonationDate.Value.Day // Thêm Day vào GroupBy
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Day = g.Key.Day, // Thêm Day vào kết quả
                    TotalAmount = g.Sum(d => d.Amount ?? 0)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.Day)
                .ToList();

            Console.WriteLine($"Donation Statistics Data: {Newtonsoft.Json.JsonConvert.SerializeObject(data)}");

            return data.Select(x => (dynamic)x).ToList();
        }
        #endregion



        #region Số lượt đăng kí theo sự kiện
        #endregion


        #region Số tiền ủng hộ theo sự kiện
        #endregion
    }
}
