using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Interfaces
{
    public interface IFavoriteService
    {
        Task<List<string>> GetFavoritesAsync();
        Task<bool> IsFavoriteAsync(string coinId);
        Task AddFavoriteAsync(string coinId);
        Task RemoveFavoriteAsync(string coinId);
        Task SaveFavoritesAsync(List<string> favorites);
    }
}
