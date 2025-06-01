using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Volunteer_website.ViewModels;

namespace Volunteer_website.Controllers
{
    public class BlogController : Controller
    {
        private readonly HttpClient _httpClient;
        private const int PageSize = 6; // Number of articles per page
        private const int MaxArticles = 100; // Maximum number of articles to fetch

        public BlogController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> Index(int page = 1, string searchString = null)
        {
            var articles = await FetchVolunteerArticles();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                articles = articles
                    .Where(a => a.Title?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }

            // Manual pagination
            var totalArticles = articles.Count;
            var totalPages = (int)Math.Ceiling(totalArticles / (double)PageSize);
            page = Math.Max(1, Math.Min(page, totalPages)); // Ensure page is within bounds
            var paginatedArticles = articles
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.SearchString = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(paginatedArticles);
        }

        private async Task<List<Article>> FetchVolunteerArticles()
        {
            var rssFeeds = new List<(string Url, bool IsVietnamese)>
            {
                ("https://vnexpress.net/rss/tin-moi-nhat.rss", true), // Vietnamese sources first
                ("https://tuoitre.vn/rss/tin-moi-nhat.rss", true),
                ("https://thanhnien.vn/rss/viet-nam.rss", true),
                ("https://zingnews.vn/rss/tin-moi-nhat.rss", true),
                ("https://www.globalgiving.org/rss-feed.xml", false), // Non-Vietnamese sources after
                ("https://reliefweb.int/headlines.xml", false),
                ("https://www.volunteerforever.com/feed/", false),
                ("https://www.unv.org/rss.xml", false),
                ("https://www.unicef.org/rss.xml", false),
                ("https://www.redcross.org/content/dam/redcross/rss/press.rss", false),
                ("https://www.who.int/feeds/entity/hac/en/rss.xml", false)
            };

            var excludeKeywords = new List<string>
            {
                "bóng đá", "ca sĩ", "showbiz", "giải trí", "thể thao", "phim", "sao việt", "chính trị", "giáo dục", "công nghệ",
                "sports", "entertainment", "politics", "economy", "technology", "finance", "weather", "fashion", "celebrity"
            };

            var disasterKeywords = new[] { "nạn nhân", "bão", "lũ", "thiên tai", "sạt lở", "động đất", "cháy rừng", "mưa lớn", "hạn hán", "lốc xoáy" };

            var vnPrimary = new[] { "tình nguyện" }; // Single keyword for Vietnamese
            var enPrimary = new[] { "volunteer" };   // Single keyword for non-Vietnamese

            var resultArticles = new List<(Article Article, bool IsVietnamese)>();

            foreach (var (url, isVietnamese) in rssFeeds)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode) continue;

                    var content = await response.Content.ReadAsStringAsync();
                    var xml = XDocument.Parse(content);
                    var items = xml.Descendants("item");

                    string source = GetSourceName(url);
                    var primaryKeywords = isVietnamese ? vnPrimary : enPrimary;

                    // Increase limit for Vietnamese feeds to maximize Vietnamese articles
                    int maxItems = isVietnamese ? 50 : 10; // Higher limit for Vietnamese sources
                    int itemCount = 0;

                    foreach (var item in items)
                    {
                        if (itemCount >= maxItems) break; // Limit per feed

                        var title = item.Element("title")?.Value?.Trim() ?? "";
                        var descriptionRaw = item.Element("description")?.Value ?? "";
                        var contentEncoded = item.Element("{http://purl.org/rss/1.0/modules/content/}encoded")?.Value ?? "";

                        var description = Regex.Replace(descriptionRaw, "<[^>]+>", "").Trim();
                        var contentText = Regex.Replace(contentEncoded, "<[^>]+>", "").Trim();

                        if (contentText.Length > description.Length)
                            description = contentText;

                        string combinedContent = $"{title} {description}";
                        if (string.IsNullOrWhiteSpace(combinedContent)) continue;

                        bool hasPrimaryKeyword = primaryKeywords.Any(k =>
                            Regex.IsMatch(combinedContent, $@"\b{k}\b", RegexOptions.IgnoreCase));

                        bool hasExcludedKeyword = excludeKeywords.Any(k =>
                            combinedContent.Contains(k, StringComparison.OrdinalIgnoreCase));

                        bool hasDisasterKeyword = disasterKeywords.Any(k =>
                            combinedContent.Contains(k, StringComparison.OrdinalIgnoreCase));

                        if (!hasPrimaryKeyword || hasExcludedKeyword || hasDisasterKeyword)
                            continue;

                        var link = item.Element("link")?.Value ?? "";
                        var pubDate = item.Element("pubDate")?.Value ?? DateTime.Now.ToString("R");

                        string imageUrl = item.Descendants()
                            .Where(e => e.Name.LocalName == "thumbnail" && e.Name.Namespace == "http://search.yahoo.com/mrss/")
                            .Select(e => e.Attribute("url")?.Value)
                            .FirstOrDefault();

                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            var enclosure = item.Element("enclosure");
                            imageUrl = enclosure?.Attribute("url")?.Value;
                        }

                        string placeholderImage = "https://static.vecteezy.com/system/resources/previews/015/779/127/original/colored-volunteer-crowd-hands-hand-drawing-lettering-volunteering-raised-hand-silhouettes-volunteer-education-poster-mockup-donation-and-charity-concept-vector.jpg";
                        if (string.IsNullOrWhiteSpace(link)) continue;

                        resultArticles.Add((new Article
                        {
                            Title = title,
                            Description = description.Length > 250 ? description.Substring(0, 250) + "..." : description,
                            Url = link,
                            PublishedAt = pubDate,
                            Source = source,
                            UrlToImage = string.IsNullOrEmpty(imageUrl) ? placeholderImage : imageUrl
                        }, isVietnamese));

                        itemCount++;
                    }
                }
                catch
                {
                    continue; // Skip if there's an error
                }
            }

            return resultArticles
                .OrderByDescending(x => x.IsVietnamese) // Prioritize Vietnamese articles
                .ThenByDescending(x => DateTime.TryParse(x.Article.PublishedAt, out var date) ? date : DateTime.MinValue)
                .Take(MaxArticles)
                .Select(x => x.Article)
                .ToList();
        }

        private string GetSourceName(string url)
        {
            if (url.Contains("vnexpress")) return "VN Express";
            if (url.Contains("tuoitre")) return "Tuổi Trẻ";
            if (url.Contains("thanhnien")) return "Thanh Niên";
            if (url.Contains("zingnews")) return "Zing News";
            if (url.Contains("globalgiving")) return "GlobalGiving";
            if (url.Contains("reliefweb")) return "ReliefWeb";
            if (url.Contains("volunteerforever")) return "Volunteer Forever";
            if (url.Contains("unv")) return "UN Volunteers";
            if (url.Contains("unicef")) return "UNICEF";
            if (url.Contains("redcross")) return "Red Cross";
            if (url.Contains("who")) return "WHO";
            return "Other";
        }
    }
}