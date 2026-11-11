using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                return ParseNews(json);
            }
            catch
            {
                return new List<NewsArticle>();
            }
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
