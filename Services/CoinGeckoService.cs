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
    public class CoinGeckoService : ICoinGeckoService
    {
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;
        private const string BaseUrl = "https://api.coingecko.com/api/v3";

        public CoinGeckoService(HttpClient httpClient, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _settingsService = settingsService;
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        private string GetApiKey()
        {
            try { return _settingsService.GetSettingsAsync().GetAwaiter().GetResult().CoinGeckoApiKey ?? string.Empty; }
            catch { return string.Empty; }
        }

        private void ApplyApiKey()
        {
            _httpClient.DefaultRequestHeaders.Remove("x-cg-demo-api-key");
            var key = GetApiKey();
            if (!string.IsNullOrWhiteSpace(key))
                _httpClient.DefaultRequestHeaders.Add("x-cg-demo-api-key", key);
        }

        public async Task<List<Coin>> GetTopCoinsAsync(int perPage = 100, CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/coins/markets?vs_currency=usd&order=market_cap_desc&per_page={perPage}&page=1&sparkline=true&price_change_percentage=1h%2C24h%2C7d";
            ApplyApiKey();
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            return ParseCoinList(json);
        }

        public async Task<Coin?> GetCoinDetailAsync(string coinId, CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/coins/{coinId}?localization=false&tickers=false&market_data=true&community_data=false";
            ApplyApiKey();
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            return ParseCoinDetail(json, coinId);
        }

        public async Task<Dictionary<string, Dictionary<string, decimal>>> GetSimplePriceAsync(List<string> coinIds, CancellationToken ct = default)
        {
            var ids = string.Join(",", coinIds);
            var url = $"{BaseUrl}/simple/price?vs_currencies=usd&ids={ids}";
            ApplyApiKey();
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, decimal>>>(json) ?? new();
        }

        public async Task<List<Dictionary<string, object>>> GetMarketChartAsync(string coinId, int days = 7, CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/coins/{coinId}/market_chart?vs_currency=usd&days={days}";
            ApplyApiKey();
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            var result = new List<Dictionary<string, object>>();
            if (data != null && data.TryGetValue("prices", out var prices))
            {
                foreach (var item in prices.EnumerateArray())
                {
                    var entry = new Dictionary<string, object>
                    {
                        ["timestamp"] = item[0].GetInt64(),
                        ["price"] = item[1].GetDecimal()
                    };
                    result.Add(entry);
                }
            }
            return result;
        }

        public async Task<List<Coin>> SearchCoinsAsync(string query, CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/search?query={Uri.EscapeDataString(query)}";
            ApplyApiKey();
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            var coins = new List<Coin>();
            if (data != null && data.TryGetValue("coins", out var coinsElement))
            {
                foreach (var item in coinsElement.EnumerateArray())
                {
                    var coin = new Coin
                    {
                        Id = item.GetProperty("id").GetString() ?? "",
                        Name = item.GetProperty("name").GetString() ?? "",
                        Symbol = item.GetProperty("symbol").GetString() ?? "",
                        Image = item.TryGetProperty("thumb", out var thumb) ? thumb.GetString() ?? "" : "",
                        MarketCapRank = item.TryGetProperty("market_cap_rank", out var rank) && rank.ValueKind != JsonValueKind.Null ? rank.GetInt32() : 0,
                    };
                    coins.Add(coin);
                }
            }
            return coins;
        }

        public async Task<Dictionary<string, object>> GetGlobalDataAsync(CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/global";
            ApplyApiKey();
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var root = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            var result = new Dictionary<string, object>();
            if (root != null && root.TryGetValue("data", out var data))
            {
                if (data.TryGetProperty("total_market_cap", out var tmc) && tmc.TryGetProperty("usd", out var tmcUsd))
                    result["total_market_cap"] = tmcUsd.GetDecimal();
                if (data.TryGetProperty("total_volume", out var tv) && tv.TryGetProperty("usd", out var tvUsd))
                    result["total_volume"] = tvUsd.GetDecimal();
                if (data.TryGetProperty("market_cap_percentage", out var mcp) && mcp.TryGetProperty("btc", out var btcPct))
                    result["btc_dominance"] = btcPct.GetDecimal();
                if (data.TryGetProperty("market_cap_change_percentage_24h_usd", out var change))
                    result["market_cap_change_24h"] = change.GetDecimal();
            }
            return result;
        }

        public async Task<(int value, string label)> GetFearGreedIndexAsync(CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("https://api.alternative.me/fng/?limit=1", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
                {
                    var item = data[0];
                    var value = item.GetProperty("value").GetString() ?? "0";
                    var label = item.GetProperty("value_classification").GetString() ?? "";
                    return (int.TryParse(value, out var v) ? v : 0, label);
                }
            }
            catch { }
            return (0, "");
        }

        private List<Coin> ParseCoinList(string json)
        {
            var coins = new List<Coin>();
            var docs = JsonSerializer.Deserialize<List<JsonElement>>(json);
            if (docs == null) return coins;

            foreach (var item in docs)
            {
                var coin = new Coin
                {
                    Id = item.GetProperty("id").GetString() ?? "",
                    Symbol = item.GetProperty("symbol").GetString() ?? "",
                    Name = item.GetProperty("name").GetString() ?? "",
                    Image = item.TryGetProperty("image", out var img) ? img.GetString() ?? "" : "",
                    CurrentPrice = item.TryGetProperty("current_price", out var cp) && cp.ValueKind != JsonValueKind.Null ? cp.GetDecimal() : 0,
                    MarketCap = item.TryGetProperty("market_cap", out var mc) && mc.ValueKind != JsonValueKind.Null ? (long)mc.GetDecimal() : 0,
                    MarketCapRank = item.TryGetProperty("market_cap_rank", out var mcr) && mcr.ValueKind != JsonValueKind.Null ? mcr.GetInt32() : 0,
                    TotalVolume = item.TryGetProperty("total_volume", out var tv) && tv.ValueKind != JsonValueKind.Null ? tv.GetDecimal() : 0,
                    High24H = item.TryGetProperty("high_24h", out var hi) && hi.ValueKind != JsonValueKind.Null ? hi.GetDecimal() : 0,
                    Low24H = item.TryGetProperty("low_24h", out var lo) && lo.ValueKind != JsonValueKind.Null ? lo.GetDecimal() : 0,
                    PriceChangePercentage24H = item.TryGetProperty("price_change_percentage_24h", out var p24) && p24.ValueKind != JsonValueKind.Null ? p24.GetDouble() : 0,
                    PriceChangePercentage1H = item.TryGetProperty("price_change_percentage_1h_in_currency", out var p1) && p1.ValueKind != JsonValueKind.Null ? p1.GetDouble() : 0,
                    PriceChangePercentage7D = item.TryGetProperty("price_change_percentage_7d_in_currency", out var p7) && p7.ValueKind != JsonValueKind.Null ? p7.GetDouble() : 0,
                    CirculatingSupply = item.TryGetProperty("circulating_supply", out var cs) && cs.ValueKind != JsonValueKind.Null ? cs.GetDecimal() : 0,
                    TotalSupply = item.TryGetProperty("total_supply", out var ts) && ts.ValueKind != JsonValueKind.Null ? ts.GetDecimal() : null,
                    MaxSupply = item.TryGetProperty("max_supply", out var ms) && ms.ValueKind != JsonValueKind.Null ? ms.GetDecimal() : null,
                    Ath = item.TryGetProperty("ath", out var ath) && ath.ValueKind != JsonValueKind.Null ? ath.GetDecimal() : 0,
                    AthDate = item.TryGetProperty("ath_date", out var athd) ? athd.GetString() ?? "" : "",
                    Atl = item.TryGetProperty("atl", out var atl) && atl.ValueKind != JsonValueKind.Null ? atl.GetDecimal() : 0,
                    AtlDate = item.TryGetProperty("atl_date", out var atld) ? atld.GetString() ?? "" : "",
                    LastPrice = item.TryGetProperty("current_price", out var lcp) && lcp.ValueKind != JsonValueKind.Null ? lcp.GetDecimal() : 0,
                };

                if (item.TryGetProperty("sparkline_in_7d", out var spark) && spark.ValueKind == JsonValueKind.Object)
                {
                    if (spark.TryGetProperty("price", out var prices))
                    {
                        var list = new List<double>();
                        foreach (var p in prices.EnumerateArray())
                            list.Add(p.GetDouble());
                        coin.Sparkline = list;
                    }
                }

                coins.Add(coin);
            }
            return coins;
        }

        private Coin? ParseCoinDetail(string json, string coinId)
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            var coin = new Coin { Id = coinId };

            if (doc.TryGetProperty("symbol", out var sym))
                coin.Symbol = sym.GetString() ?? "";
            if (doc.TryGetProperty("name", out var name))
                coin.Name = name.GetString() ?? "";
            if (doc.TryGetProperty("image", out var img) && img.TryGetProperty("large", out var large))
                coin.Image = large.GetString() ?? "";

            if (doc.TryGetProperty("market_data", out var md))
            {
                if (md.TryGetProperty("current_price", out var cp) && cp.TryGetProperty("usd", out var cpUsd))
                    coin.CurrentPrice = cpUsd.GetDecimal();
                if (md.TryGetProperty("market_cap", out var mc) && mc.TryGetProperty("usd", out var mcUsd))
                    coin.MarketCap = (long)mcUsd.GetDecimal();
                if (md.TryGetProperty("market_cap_rank", out var mcr))
                    coin.MarketCapRank = mcr.GetInt32();
                if (md.TryGetProperty("total_volume", out var tv) && tv.TryGetProperty("usd", out var tvUsd))
                    coin.TotalVolume = tvUsd.GetDecimal();
                if (md.TryGetProperty("high_24h", out var hi) && hi.TryGetProperty("usd", out var hiUsd))
                    coin.High24H = hiUsd.GetDecimal();
                if (md.TryGetProperty("low_24h", out var lo) && lo.TryGetProperty("usd", out var loUsd))
                    coin.Low24H = loUsd.GetDecimal();
                if (md.TryGetProperty("price_change_percentage_24h", out var p24))
                    coin.PriceChangePercentage24H = p24.GetDouble();
                if (md.TryGetProperty("circulating_supply", out var cs))
                    coin.CirculatingSupply = cs.GetDecimal();
                if (md.TryGetProperty("total_supply", out var ts))
                    coin.TotalSupply = ts.GetDecimal();
                if (md.TryGetProperty("max_supply", out var ms))
                    coin.MaxSupply = ms.GetDecimal();
                if (md.TryGetProperty("ath", out var ath) && ath.TryGetProperty("usd", out var athUsd))
                    coin.Ath = athUsd.GetDecimal();
                if (md.TryGetProperty("ath_date", out var athd) && athd.TryGetProperty("usd", out var athdUsd))
                    coin.AthDate = athdUsd.GetString() ?? "";
                if (md.TryGetProperty("atl", out var atl) && atl.TryGetProperty("usd", out var atlUsd))
                    coin.Atl = atlUsd.GetDecimal();
                if (md.TryGetProperty("atl_date", out var atld) && atld.TryGetProperty("usd", out var atldUsd))
                    coin.AtlDate = atldUsd.GetString() ?? "";
            }

            coin.LastPrice = coin.CurrentPrice;
            return coin;
        }
    }
}
