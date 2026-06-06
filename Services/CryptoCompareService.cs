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
        private const string BaseUrl = "https://min-api.cryptocompare.com/data";
        private const string ApiKey = "178dfcdab7b6d31d62bec50e5819cd22aa2f5352a3b1b9dee5949bb928a70c60";

        public CryptoCompareService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Apikey {ApiKey}");
        }

        public async Task<List<NewsArticle>> GetLatestNewsAsync(string categories = "", CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/v2/news/?lang=EN";
            if (!string.IsNullOrEmpty(categories))
                url += $"&categories={Uri.EscapeDataString(categories)}";
            url += "&sortOrder=latest";

            try
            {
                var response = await _httpClient.GetAsync(url, ct);
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
