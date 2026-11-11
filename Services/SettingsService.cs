using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Interfaces;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _filePath;
        private AppSettings? _cached;
        private DateTime _cacheTime = DateTime.MinValue;

        public SettingsService()
        {
            var appData = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "CryptoTrack");
            Directory.CreateDirectory(appData);
            _filePath = Path.Combine(appData, "settings.json");
        }

        public async Task<AppSettings> GetSettingsAsync()
        {
            // Cache for a short period so hot paths (every API request) do not
            // re-read the file. Forced refresh happens right after saving.
            if (_cached != null && (DateTime.UtcNow - _cacheTime).TotalSeconds < 2)
                return _cached;

            if (!File.Exists(_filePath))
            {
                _cached = new AppSettings();
                _cacheTime = DateTime.UtcNow;
                return _cached;
            }

            var json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
            _cached = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            _cacheTime = DateTime.UtcNow;
            return _cached;
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json).ConfigureAwait(false);
            _cached = settings;
            _cacheTime = DateTime.UtcNow;
        }
    }
}
