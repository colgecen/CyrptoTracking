using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AvaloniaAppCrypto.Services
{
    /// <summary>
    /// Simple in-memory bitmap cache for coin icons downloaded from CoinGecko.
    /// </summary>
    public static class ImageLoader
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();
        private static readonly ConcurrentDictionary<string, IImage?> Cache = new();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoTrack/1.0 (+https://github.com/colgecen/cyrpto-tracking-app)");
            client.Timeout = TimeSpan.FromSeconds(10);
            return client;
        }

        public static async Task<IImage?> GetAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (Cache.TryGetValue(url, out var cached))
                return cached;

            try
            {
                using var response = await HttpClient.GetAsync(url).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    Cache[url] = null;
                    return null;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                using var stream = new MemoryStream(bytes);
                var bitmap = new Bitmap(stream);
                Cache[url] = bitmap;
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
