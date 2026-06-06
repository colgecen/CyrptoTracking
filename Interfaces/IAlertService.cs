using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Interfaces
{
    public interface IAlertService
    {
        Task<List<PriceAlert>> GetAlertsAsync();
        Task AddAlertAsync(PriceAlert alert);
        Task UpdateAlertAsync(PriceAlert alert);
        Task RemoveAlertAsync(string alertId);
        Task<List<PriceAlert>> CheckAlertsAsync(Dictionary<string, decimal> prices);
        Task ClearTriggeredAsync();
    }
}
