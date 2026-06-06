using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Interfaces;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly string _filePath;
        private List<PortfolioEntry> _entries = new();

        public PortfolioService()
        {
            var appData = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "CryptoTrack");
            Directory.CreateDirectory(appData);
            _filePath = Path.Combine(appData, "portfolio.json");
        }

        private async Task LoadAsync()
        {
            if (!File.Exists(_filePath))
            {
                _entries = new List<PortfolioEntry>();
                return;
            }
            var json = await File.ReadAllTextAsync(_filePath);
            _entries = JsonSerializer.Deserialize<List<PortfolioEntry>>(json) ?? new List<PortfolioEntry>();
        }

        private async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<PortfolioEntry>> GetPortfolioAsync()
        {
            await LoadAsync();
            return new List<PortfolioEntry>(_entries);
        }

        public async Task AddEntryAsync(PortfolioEntry entry)
        {
            await LoadAsync();
            _entries.Add(entry);
            await SaveAsync();
        }

        public async Task UpdateEntryAsync(PortfolioEntry entry)
        {
            await LoadAsync();
            var idx = _entries.FindIndex(e => e.Id == entry.Id);
            if (idx >= 0)
            {
                _entries[idx] = entry;
                await SaveAsync();
            }
        }

        public async Task RemoveEntryAsync(string entryId)
        {
            await LoadAsync();
            _entries.RemoveAll(e => e.Id == entryId);
            await SaveAsync();
        }

        public async Task ClearAllAsync()
        {
            _entries.Clear();
            await SaveAsync();
        }
    }
}
