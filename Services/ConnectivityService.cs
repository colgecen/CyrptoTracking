using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaAppCrypto.Interfaces;

namespace AvaloniaAppCrypto.Services
{
    public class ConnectivityService : IConnectivityService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private Timer? _timer;
        private bool _isOnline = true;

        public bool IsOnline => _isOnline;
        public event Action<bool>? ConnectivityChanged;

        public ConnectivityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task CheckConnectivityAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://api.coingecko.com/api/v3/ping");
                var wasOnline = _isOnline;
                _isOnline = response.IsSuccessStatusCode;
                if (wasOnline != _isOnline)
                    ConnectivityChanged?.Invoke(_isOnline);
            }
            catch
            {
                var wasOnline = _isOnline;
                _isOnline = false;
                if (wasOnline != _isOnline)
                    ConnectivityChanged?.Invoke(_isOnline);
            }
        }

        public void Start()
        {
            _timer = new Timer(async _ => await CheckConnectivityAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
