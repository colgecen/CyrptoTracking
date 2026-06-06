using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Interfaces
{
    public interface IPortfolioService
    {
        Task<List<PortfolioEntry>> GetPortfolioAsync();
        Task AddEntryAsync(PortfolioEntry entry);
        Task UpdateEntryAsync(PortfolioEntry entry);
        Task RemoveEntryAsync(string entryId);
        Task ClearAllAsync();
    }
}
