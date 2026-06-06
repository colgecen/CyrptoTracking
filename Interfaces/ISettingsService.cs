using System.Threading.Tasks;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Interfaces
{
    public interface ISettingsService
    {
        Task<AppSettings> GetSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);
    }
}
