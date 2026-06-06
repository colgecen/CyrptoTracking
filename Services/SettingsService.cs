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
            if (!File.Exists(_filePath))
                return new AppSettings();

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
