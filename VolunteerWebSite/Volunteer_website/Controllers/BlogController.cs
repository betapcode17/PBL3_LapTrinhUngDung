using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Volunteer_website.Controllers
{
    public class BlogController : Controller
    {
        private readonly HttpClient _httpClient;

        public BlogController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> Index()
        {
            var articles = await FetchVolunteerArticles();
            return View(articles);
        }

        private async Task<List<Article>> FetchVolunteerArticles()
        {
            try
            {
                var rssUrls = new List<(string Url, bool IsVietnamese)>
                {
                    // Vietnam
                    ("https://vnexpress.net/rss/tin-moi-nhat.rss", true),
                    ("https://tuoitre.vn/rss/tin-moi-nhat.rss", true),
                    ("https://thanhnien.vn/rss/viet-nam.rss", true),
                    ("https://zingnews.vn/rss/tin-moi-nhat.rss", true),

                    // Global volunteer and charity
                    ("https://www.globalgiving.org/rss-feed.xml", false),
                    ("https://reliefweb.int/headlines.xml", false),
                    ("https://www.volunteerforever.com/feed/", false),
                    ("https://www.unv.org/rss.xml", false),
                    ("https://www.unicef.org/rss.xml", false),
                    ("https://www.redcross.org/content/dam/redcross/rss/press.rss", false),
                    ("https://www.charitynavigator.org/index.cfm?bay=content.view&cpid=43", false), // summary
                    ("https://www.who.int/feeds/entity/hac/en/rss.xml", false)
                };

                var vietnamesePrimaryKeywords = new List<string> { "tình nguyện", "từ thiện", "hiến máu", "thiện nguyện", "cứu trợ" };
                var vietnameseSecondaryKeywords = new List<string> { "giúp đỡ", "ủng hộ", "hỗ trợ", "khó khăn", "mồ côi", "gương sáng", "cộng đồng" };

                var englishPrimaryKeywords = new List<string> { "volunteer", "charity", "nonprofit", "philanthropy", "humanitarian", "relief aid" };
                var englishSecondaryKeywords = new List<string> { "donation", "blood drive", "fundraising", "support community", "emergency aid", "orphan", "food bank", "helping" };

                var excludeKeywords = new List<string>
                {
                    "bóng đá", "ca sĩ", "showbiz", "giải trí", "thể thao", "phim", "sao việt", "chính trị", "giáo dục", "công nghệ",
                    "sports", "entertainment", "politics", "economy", "technology", "finance", "weather", "fashion", "celebrity"
                };

                var allArticles = new List<Article>();

                foreach (var (Url, IsVietnamese) in rssUrls)
                {
                    HttpResponseMessage response;

                    try
                    {
                        response = await _httpClient.GetAsync(Url);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"❌ Không thể truy cập RSS: {Url} | {e.Message}");
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"❌ Lỗi lấy RSS từ {Url}: {response.StatusCode}");
                        continue;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var xmlDoc = XDocument.Parse(content);
                    var items = xmlDoc.Descendants("item");

                    string sourceName = Url.Contains("vnexpress") ? "VN Express" :
                                        Url.Contains("tuoitre") ? "Tuổi Trẻ" :
                                        Url.Contains("thanhnien") ? "Thanh Niên" :
                                        Url.Contains("zingnews") ? "Zing News" :
                                        Url.Contains("globalgiving") ? "GlobalGiving" :
                                        Url.Contains("reliefweb") ? "ReliefWeb" :
                                        Url.Contains("volunteerforever") ? "Volunteer Forever" :
                                        Url.Contains("unv") ? "UN Volunteers" :
                                        Url.Contains("unicef") ? "UNICEF" :
                                        Url.Contains("redcross") ? "Red Cross" :
                                        Url.Contains("who") ? "WHO" : "Other";

                    var primary = IsVietnamese ? vietnamesePrimaryKeywords : englishPrimaryKeywords;
                    var secondary = IsVietnamese ? vietnameseSecondaryKeywords : englishSecondaryKeywords;

                    foreach (var item in items)
                    {
                        var title = item.Element("title")?.Value ?? "";
                        var description = Regex.Replace(item.Element("description")?.Value ?? "", "<[^>]+>", "").Trim();
                        var link = item.Element("link")?.Value ?? "";
                        var pubDate = item.Element("pubDate")?.Value ?? DateTime.Now.ToString("R");

                        var fullContent = title + " " + description;

                        // Kiểm tra lọc
                        bool hasPrimary = primary.Any(k => fullContent.Contains(k, StringComparison.OrdinalIgnoreCase));
                        bool hasSecondary = secondary.Any(k => fullContent.Contains(k, StringComparison.OrdinalIgnoreCase));
                        bool hasExcluded = excludeKeywords.Any(k => fullContent.Contains(k, StringComparison.OrdinalIgnoreCase));

                        if (string.IsNullOrWhiteSpace(link) || !hasPrimary || !hasSecondary || hasExcluded)
                            continue;

                        allArticles.Add(new Article
                        {
                            Title = title,
                            Description = description.Length > 250 ? description.Substring(0, 250) + "..." : description,
                            Url = link,
                            PublishedAt = pubDate,
                            Source = sourceName
                        });
                    }
                }

                var sortedArticles = allArticles.OrderByDescending(a =>
                {
                    try { return DateTime.Parse(a.PublishedAt); }
                    catch { return DateTime.Now; }
                }).Take(30).ToList();

                Console.WriteLine($"✅ Tổng bài viết phù hợp: {sortedArticles.Count}");

                if (!sortedArticles.Any())
                {
                    ViewBag.ErrorMessage = "Không tìm thấy bài viết phù hợp về tình nguyện, từ thiện.";
                }

                return sortedArticles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi hệ thống: {ex.Message}");
                ViewBag.ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
                return new List<Article>();
            }
        }
    }

    public class Article
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? PublishedAt { get; set; }
        public string? Source { get; set; }
    }
}
