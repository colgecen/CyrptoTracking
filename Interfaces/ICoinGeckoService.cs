using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Interfaces
{
    public interface ICoinGeckoService
    {
        Task<List<Coin>> GetTopCoinsAsync(int perPage = 100, CancellationToken ct = default);
        Task<Coin?> GetCoinDetailAsync(string coinId, CancellationToken ct = default);
        Task<Dictionary<string, Dictionary<string, decimal>>> GetSimplePriceAsync(List<string> coinIds, CancellationToken ct = default);
        Task<List<Dictionary<string, object>>> GetMarketChartAsync(string coinId, int days = 7, CancellationToken ct = default);
        Task<List<Coin>> SearchCoinsAsync(string query, CancellationToken ct = default);
        Task<Dictionary<string, object>> GetGlobalDataAsync(CancellationToken ct = default);
        Task<(int value, string label)> GetFearGreedIndexAsync(CancellationToken ct = default);
    }
}
