using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AvaloniaAppCrypto.Interfaces;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Services
{
    public class CryptoCompareService : ICryptoCompareService
    {
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;
        private const string BaseUrl = "https://min-api.cryptocompare.com/data";

        public CryptoCompareService(HttpClient httpClient, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _settingsService = settingsService;
            _httpClient.Timeout = Timeout.InfiniteTimeSpan;
        }

        private async Task<HttpResponseMessage> SendAsync(string url, CancellationToken ct)
        {
            var settings = await _settingsService.GetSettingsAsync().ConfigureAwait(false);
            var timeoutSeconds = settings.ApiTimeoutSeconds > 0 ? settings.ApiTimeoutSeconds : 10;

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var key = settings.CryptoCompareApiKey ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(key))
                request.Headers.TryAddWithoutValidation("Authorization", $"Apikey {key}");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            return await _httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> GetWithRetryAsync(string url, CancellationToken ct)
        {
            const int maxAttempts = 3;
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    var response = await SendAsync(url, ct).ConfigureAwait(false);
                    if ((int)response.StatusCode == 429 && attempt < maxAttempts - 1)
                    {
                        response.Dispose();
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct).ConfigureAwait(false);
                        continue;
                    }
                    return response;
                }
                catch (Exception) when (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500 * (attempt + 1)), ct).ConfigureAwait(false);
                }
            }
            return await SendAsync(url, ct).ConfigureAwait(false);
        }

        public async Task<List<NewsArticle>> GetLatestNewsAsync(string categories = "", CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/v2/news/?lang=EN";
            if (!string.IsNullOrEmpty(categories))
                url += $"&categories={Uri.EscapeDataString(categories)}";
            url += "&sortOrder=latest";

            try
            {
                var response = await GetWithRetryAsync(url, ct);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    var parsed = ParseNews(json);
                    if (parsed.Count > 0)
                        return parsed;
                }
            }
            catch { }

            // CryptoCompare haber API'si artık anahtar istiyor (401). Anahtar
            // yoksa/başarısızsa herkese açık RSS kaynaklarına düş.
            return await GetLatestNewsFromRssAsync(ct);
        }

        private static readonly string[] RssFeeds =
        {
            "https://cointelegraph.com/rss",
            "https://decrypt.co/feed",
            "https://cryptoslate.com/feed/"
        };

        private async Task<List<NewsArticle>> GetLatestNewsFromRssAsync(CancellationToken ct)
        {
            var articles = new List<NewsArticle>();
            foreach (var feed in RssFeeds)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, feed);
                    request.Headers.TryAddWithoutValidation(
                        "User-Agent", "Mozilla/5.0 (compatible; CryptoTrack/1.0)");
                    request.Headers.TryAddWithoutValidation(
                        "Accept", "application/rss+xml, application/xml, text/xml, */*");

                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(12));
                    using var response = await _httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode) continue;

                    var xml = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
                    articles.AddRange(ParseRss(xml));
                }
                catch { }
            }

            return articles
                .GroupBy(a => a.Title, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderByDescending(a => a.PublishedOn)
                .ToList();
        }

        private static List<NewsArticle> ParseRss(string xml)
        {
            var articles = new List<NewsArticle>();
            try
            {
                var doc = XDocument.Parse(xml);
                var channelTitle = doc.Root?.Element("channel")?.Element("title")?.Value?.Trim();

                foreach (var item in doc.Descendants("item"))
                {
                    try
                    {
                        var title = item.Element("title")?.Value?.Trim() ?? "";
                        var link = item.Element("link")?.Value?.Trim() ?? "";
                        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(link))
                            continue;

                        var description = item.Element("description")?.Value ?? "";
                        var content = item.Elements()
                            .FirstOrDefault(e => e.Name.LocalName == "encoded")?.Value ?? "";
                        var image = item.Elements()
                                        .FirstOrDefault(e => e.Name.LocalName == "content")?.Attribute("url")?.Value
                                    ?? item.Elements()
                                        .FirstOrDefault(e => e.Name.LocalName == "thumbnail")?.Attribute("url")?.Value
                                    ?? item.Elements()
                                        .FirstOrDefault(e => e.Name.LocalName == "enclosure")?.Attribute("url")?.Value
                                    ?? ExtractFirstImage(content) 
                                    ?? ExtractFirstImage(description)
                                    ?? "";

                        var categories = string.Join(",", item.Elements("category").Select(c => c.Value.Trim()));

                        var pubText = item.Element("pubDate")?.Value;
                        var published = DateTime.TryParse(pubText, CultureInfo.InvariantCulture,
                            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt)
                            ? dt
                            : DateTime.UtcNow;

                        articles.Add(new NewsArticle
                        {
                            Title = WebUtility.HtmlDecode(title),
                            Url = link,
                            Source = channelTitle ?? new Uri(link).Host,
                            ImageUrl = image,
                            Body = string.IsNullOrWhiteSpace(description) ? content : description,
                            Categories = WebUtility.HtmlDecode(categories),
                            PublishedOn = published
                        });
                    }
                    catch { }
                }
            }
            catch { }
            return articles;
        }

        private static string? ExtractFirstImage(string? html)
        {
            if (string.IsNullOrEmpty(html)) return null;
            var match = Regex.Match(html,
                @"<img[^>]+src\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        private List<NewsArticle> ParseNews(string json)
        {
            var articles = new List<NewsArticle>();
            var root = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (root == null || !root.TryGetValue("Data", out var data)) return articles;

            foreach (var item in data.EnumerateArray())
            {
                try
                {
                    var article = new NewsArticle
                    {
                        Title = item.GetProperty("title").GetString() ?? "",
                        Source = item.GetProperty("source").GetString() ?? "",
                        Url = item.GetProperty("url").GetString() ?? "",
                        ImageUrl = item.TryGetProperty("imageurl", out var img) ? img.GetString() ?? "" : "",
                        Body = item.TryGetProperty("body", out var body) ? body.GetString() ?? "" : "",
                        Categories = item.TryGetProperty("categories", out var cats) ? cats.GetString() ?? "" : "",
                        PublishedOn = DateTimeOffset.FromUnixTimeSeconds(
                            item.GetProperty("published_on").GetInt64()).DateTime
                    };
                    articles.Add(article);
                }
                catch { }
            }
            return articles;
        }
    }
}
