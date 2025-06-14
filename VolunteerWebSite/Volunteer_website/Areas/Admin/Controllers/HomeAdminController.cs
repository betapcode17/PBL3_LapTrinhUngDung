
﻿using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Models;
using Volunteer_website.Areas.Admins.Data;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authentication;
using System.Buffers;
using Volunteer_website.Helpers;
using Volunteer_website.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
namespace Volunteer_website.Areas.Admins.Controllers
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
        public IActionResult Index(string? startDate = null, string? endDate = null)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login", "Account");
            }
            var currentAdmin = _db.Admins.FirstOrDefault(a => a.AdminId == adminId);
            if (currentAdmin == null)
                return NotFound();
            ViewBag.ImgPath = currentAdmin.ImgPath;

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

            // Lấy dữ liệu thống kê đăng ký theo thời gian
            var statisticsData = StatisticsRegistrationByTime(startDate, endDate);
            var registrationChartData = statisticsData.Select(x => new
            {
                Label = $"{x.Year}-{x.Month:D2}-{x.Day:D2}",
                Count = x.Count
            }).ToList();

            ViewBag.RegistrationStatistics = new
            {
                Labels = registrationChartData.Select(x => x.Label).ToArray(),
                Data = registrationChartData.Select(x => x.Count).ToArray()
            };

            // Lấy dữ liệu thống kê donation theo thời gian
            var donationStatistics = StatisticsDonationByTime(startDate, endDate);
            var donationChartData = donationStatistics.Select(x => new
            {
                Label = $"{x.Year}-{x.Month:D2}-{x.Day:D2}",
                TotalAmount = x.TotalAmount
            }).ToList();

            ViewBag.DonationStatistics = new
            {
                Labels = donationChartData.Select(x => x.Label).ToArray(),
                Data = donationChartData.Select(x => x.TotalAmount).ToArray()
            };

            // Lấy dữ liệu thống kê sự kiện đang hoạt động theo thời gian
            var eventIsActiveData = StatisticsEventIsActiveByTime(startDate, endDate);
            var eventIsActiveChartData = eventIsActiveData.Select(x => new
            {
                Label = $"{x.Year}-{x.Month:D2}-{x.Day:D2}",
                Count = x.Count
            }).ToList();

            ViewBag.EventIsActiveStatistics = new
            {
                Labels = eventIsActiveChartData.Select(x => x.Label).ToArray(),
                Data = eventIsActiveChartData.Select(x => x.Count).ToArray()
            };

            // Thêm dữ liệu thống kê sự kiện tổng quát
            var eventSummaryData = StatisticsEventSummary(searchValue: "", startDate: startDate, endDate: endDate);
            ViewBag.EventSummary = eventSummaryData;

            return View();
        }
        #endregion

        #region đăng ký theo thời gian
        public List<dynamic> StatisticsRegistrationByTime(string? startDate = null, string? endDate = null)
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

            var data = _db.Registrations
                .Where(r => r.Status == "ACCEPTED")
                .Where(r => r.RegisterAt.HasValue)
                .Where(r => r.RegisterAt.Value >= startDateOnly && r.RegisterAt.Value <= endDateOnly)
                .GroupBy(r => r.RegisterAt.Value)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.Date, x => x.Count);

            // Create a list of all dates in the range with counts
            var result = new List<dynamic>();
            for (var date = startDateOnly; date <= endDateOnly; date = date.AddDays(1))
            {
                result.Add(new
                {
                    Year = date.Year,
                    Month = date.Month,
                    Day = date.Day,
                    Count = data.ContainsKey(date) ? data[date] : 0
                });
            }

            return result;
        }
        #endregion

        #region Event tổ chức theo thời gian
        public List<dynamic> StatisticsEventIsActiveByTime(string? startDate = null, string? endDate = null)
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

            var eventData = _db.Events
                .Where(e => e.DayBegin.HasValue && e.DayEnd.HasValue)
                .Where(e => e.Status == "ACCEPTED")
                .Where(e => e.DayBegin.Value <= endDateOnly && e.DayEnd.Value >= startDateOnly)
                .GroupBy(e => e.DayBegin.Value)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.Date, x => x.Count);

            var result = new List<dynamic>();
            for (var date = startDateOnly; date <= endDateOnly; date = date.AddDays(1))
            {
                result.Add(new
                {
                    Year = date.Year,
                    Month = date.Month,
                    Day = date.Day,
                    Count = eventData.ContainsKey(date) ? eventData[date] : 0
                });
            }

            return result;
        }
        #endregion

        #region donation theo thời gian
        public List<dynamic> StatisticsDonationByTime(string? startDate = null, string? endDate = null)
        {
            // Xác định khoảng thời gian
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

            // Lấy dữ liệu, group theo ngày và tính tổng Amount
            var data = _db.Donations
                .Where(d => d.DonationDate.HasValue)
                .AsEnumerable() // xử lý DateOnly ở client-side
                .Where(d => DateOnly.FromDateTime(d.DonationDate.Value) >= startDateOnly
                        && DateOnly.FromDateTime(d.DonationDate.Value) <= endDateOnly)
                .GroupBy(d => DateOnly.FromDateTime(d.DonationDate.Value))
                .Select(g => new
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(x => x.Amount)
                })
                .ToDictionary(x => x.Date, x => x.TotalAmount);

            // Tạo danh sách tất cả ngày trong khoảng, nếu không có thì mặc định 0
            var result = new List<dynamic>();
            for (var date = startDateOnly; date <= endDateOnly; date = date.AddDays(1))
            {
                result.Add(new
                {
                    Year = date.Year,
                    Month = date.Month,
                    Day = date.Day,
                    TotalAmount = data.ContainsKey(date) ? data[date] : 0
                });
            }

            return result;
        }
        #endregion


        #region Bảng thống kê tổng quát
        public List<dynamic> StatisticsEventSummary(string searchValue = "", string? startDate = null, string? endDate = null)
        {
            DateTime startDateTime, endDateTime;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                startDateTime = DateTime.Parse(startDate);
                endDateTime = DateTime.Parse(endDate);
            }
            else
            {
                endDateTime = DateTime.Now;
                startDateTime = endDateTime.AddDays(-30);
            }

            var eventQuery = _db.Events.AsQueryable(); // Loại bỏ điều kiện orgId
            if (!string.IsNullOrEmpty(searchValue))
            {
                eventQuery = eventQuery.Where(e => e.Name != null && e.Name.Contains(searchValue));
            }

            var events = eventQuery.ToList();

            var registrationCounts = _db.Registrations
                .Where(r => events.Select(e => e.EventId).Contains(r.EventId) && r.RegisterAt.HasValue)
                .AsEnumerable()
                .Where(r => r.RegisterAt!.Value.ToDateTime(TimeOnly.MinValue) >= startDateTime &&
                            r.RegisterAt!.Value.ToDateTime(TimeOnly.MinValue) <= endDateTime)
                .GroupBy(r => r.EventId)
                .Select(g => new { EventId = g.Key ?? string.Empty, Count = g.Count() })
                .ToDictionary(g => g.EventId, g => g.Count);

            var donationAmounts = _db.Donations
                .Where(d => events.Select(e => e.EventId).Contains(d.EventId) && d.DonationDate.HasValue)
                .AsEnumerable()
                .Where(d => d.DonationDate!.Value >= startDateTime && d.DonationDate!.Value <= endDateTime)
                .GroupBy(d => d.EventId)
                .Select(g => new { EventId = g.Key ?? string.Empty, TotalAmount = g.Sum(d => d.Amount ?? 0) })
                .ToDictionary(g => g.EventId, g => g.TotalAmount);

            var data = events.Select(e => new
            {
                EventName = e.Name ?? "Sự kiện không tên",
                RegistrationCount = registrationCounts.ContainsKey(e.EventId) ? registrationCounts[e.EventId] : 0,
                DonationAmount = donationAmounts.ContainsKey(e.EventId) ? donationAmounts[e.EventId] : 0
            })
            .OrderBy(x => x.EventName)
            .ToList();

            Console.WriteLine($"Event Summary Statistics: {Newtonsoft.Json.JsonConvert.SerializeObject(data)}");

            return data.Cast<dynamic>().ToList();
        }
        #endregion

        #region Xuất Excel
        [HttpGet]
        [Route("ExportEventSummaryReport")]
        public IActionResult ExportEventSummaryReport(string searchValue = "", string? startDate = null, string? endDate = null)
        {
            Console.WriteLine($"ExportEventSummaryReport called with searchValue: {searchValue}, startDate: {startDate}, endDate: {endDate}");

            var data = StatisticsEventSummary(searchValue, startDate, endDate);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Thống kê sự kiện");

                worksheet.Cells[1, 1].Value = "Thống kê tổng hợp sự kiện";
                worksheet.Cells[1, 1, 1, 3].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                string formattedStartDate = string.IsNullOrEmpty(startDate) ? DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy") : DateOnly.Parse(startDate).ToString("dd/MM/yyyy");
                string formattedEndDate = string.IsNullOrEmpty(endDate) ? DateTime.Now.ToString("dd/MM/yyyy") : DateOnly.Parse(endDate).ToString("dd/MM/yyyy");
                worksheet.Cells[2, 1].Value = $"Thống kê từ ngày {formattedStartDate} đến ngày {formattedEndDate}";
                worksheet.Cells[2, 1, 2, 3].Merge = true;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 1].Value = "Tên sự kiện";
                worksheet.Cells[3, 2].Value = "Số lượt tham gia";
                worksheet.Cells[3, 3].Value = "Số tiền ủng hộ (VND)";
                worksheet.Cells[3, 1, 3, 3].Style.Font.Bold = true;
                worksheet.Cells[3, 1, 3, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, 1, 3, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[3, 1, 3, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                int row = 4;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.EventName;
                    worksheet.Cells[row, 2].Value = item.RegistrationCount;
                    worksheet.Cells[row, 3].Value = item.DonationAmount;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                    row++;
                }

                worksheet.Cells[3, 1, row - 1, 3].AutoFitColumns();

                worksheet.Cells[3, 1, row - 1, 3].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, 1, row - 1, 3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, 1, row - 1, 3].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, 1, row - 1, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"ThongKeSuKien_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
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

