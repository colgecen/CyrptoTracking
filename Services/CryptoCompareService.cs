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
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        private async Task ApplyApiKeyAsync()
        {
            var settings = await _settingsService.GetSettingsAsync().ConfigureAwait(false);
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            var key = settings.CryptoCompareApiKey ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(key))
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Apikey {key}");
            if (settings.ApiTimeoutSeconds > 0)
                _httpClient.Timeout = TimeSpan.FromSeconds(settings.ApiTimeoutSeconds);
        }

        private async Task<System.Net.Http.HttpResponseMessage> GetWithRetryAsync(string url, System.Threading.CancellationToken ct)
        {
            var maxAttempts = 3;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    await ApplyApiKeyAsync();
                    var response = await _httpClient.GetAsync(url, ct);
                    if ((int)response.StatusCode == 429 && attempt < maxAttempts - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                        continue;
                    }
                    return response;
                }
                catch (Exception) when (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500 * (attempt + 1)), ct);
                }
            }
            await ApplyApiKeyAsync();
            return await _httpClient.GetAsync(url, ct);
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
