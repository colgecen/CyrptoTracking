using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Interfaces
{
    public interface ICryptoCompareService
    {
        Task<List<NewsArticle>> GetLatestNewsAsync(string categories = "", CancellationToken ct = default);
    }
}
