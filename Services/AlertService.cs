using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Interfaces;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Services
{
    public class AlertService : IAlertService
    {
        private readonly string _filePath;
        private List<PriceAlert> _alerts = new();

        public AlertService()
        {
            var appData = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "CryptoTrack");
            Directory.CreateDirectory(appData);
            _filePath = Path.Combine(appData, "alerts.json");
        }

        private async Task LoadAsync()
        {
            if (!File.Exists(_filePath))
            {
                _alerts = new List<PriceAlert>();
                return;
            }
            var json = await File.ReadAllTextAsync(_filePath);
            _alerts = JsonSerializer.Deserialize<List<PriceAlert>>(json) ?? new List<PriceAlert>();
        }

        private async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_alerts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<PriceAlert>> GetAlertsAsync()
        {
            await LoadAsync();
            return new List<PriceAlert>(_alerts);
        }

        public async Task AddAlertAsync(PriceAlert alert)
        {
            await LoadAsync();
            _alerts.Add(alert);
            await SaveAsync();
        }

        public async Task UpdateAlertAsync(PriceAlert alert)
        {
            await LoadAsync();
            var idx = _alerts.FindIndex(a => a.Id == alert.Id);
            if (idx >= 0)
            {
                _alerts[idx] = alert;
                await SaveAsync();
            }
        }

        public async Task RemoveAlertAsync(string alertId)
        {
            await LoadAsync();
            _alerts.RemoveAll(a => a.Id == alertId);
            await SaveAsync();
        }

        public async Task<List<PriceAlert>> CheckAlertsAsync(Dictionary<string, decimal> prices)
        {
            await LoadAsync();
            var triggered = new List<PriceAlert>();
            foreach (var alert in _alerts)
            {
                if (!alert.IsActive) continue;
                if (prices.TryGetValue(alert.CoinId.ToLower(), out var currentPrice))
                {
                    bool shouldTrigger = alert.Condition == "Above"
                        ? currentPrice >= alert.TargetPrice
                        : currentPrice <= alert.TargetPrice;

                    if (shouldTrigger)
                    {
                        alert.IsActive = false;
                        triggered.Add(alert);
                    }
                }
            }
            if (triggered.Count > 0)
                await SaveAsync();
            return triggered;
        }

        public async Task ClearTriggeredAsync()
        {
            await LoadAsync();
            _alerts.RemoveAll(a => !a.IsActive);
            await SaveAsync();
        }
    }
}
