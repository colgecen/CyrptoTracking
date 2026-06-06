using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Interfaces;

namespace AvaloniaAppCrypto.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly string _filePath;
        private List<string> _favorites = new();

        public FavoriteService()
        {
            var appData = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "CryptoTrack");
            Directory.CreateDirectory(appData);
            _filePath = Path.Combine(appData, "favorites.json");
        }

        private async Task LoadAsync()
        {
            if (!File.Exists(_filePath))
            {
                _favorites = new List<string>();
                return;
            }
            var json = await File.ReadAllTextAsync(_filePath);
            _favorites = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        private async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_favorites);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<string>> GetFavoritesAsync()
        {
            await LoadAsync();
            return new List<string>(_favorites);
        }

        public async Task<bool> IsFavoriteAsync(string coinId)
        {
            await LoadAsync();
            return _favorites.Contains(coinId);
        }

        public async Task AddFavoriteAsync(string coinId)
        {
            await LoadAsync();
            if (!_favorites.Contains(coinId))
            {
                _favorites.Add(coinId);
                await SaveAsync();
            }
        }

        public async Task RemoveFavoriteAsync(string coinId)
        {
            await LoadAsync();
            if (_favorites.Remove(coinId))
                await SaveAsync();
        }

        public async Task SaveFavoritesAsync(List<string> favorites)
        {
            _favorites = favorites;
            await SaveAsync();
        }
    }
}
