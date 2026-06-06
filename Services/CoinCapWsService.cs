using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaAppCrypto.Interfaces;
using Websocket.Client;

namespace AvaloniaAppCrypto.Services
{
    public class CoinCapWsService : ICoinCapWsService, IDisposable
    {
        private WebsocketClient? _client;
        private const string CoinCapUrl = "wss://ws.coincap.io/prices?assets=";

        public event Action<string, decimal>? PriceUpdated;
        public bool IsConnected => _client?.IsRunning ?? false;

        public async Task ConnectAsync(List<string> assets)
        {
            var url = CoinCapUrl + string.Join(",", assets);
            _client = new WebsocketClient(new Uri(url));
            _client.MessageReceived
                .Where(msg => msg.Text != null)
                .Subscribe(msg => HandleMessage(msg.Text!));
            _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            _client.ErrorReconnectTimeout = TimeSpan.FromSeconds(5);
            await _client.Start();
        }

        private void HandleMessage(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                foreach (var item in root.EnumerateObject())
                {
                    var coinId = item.Name.ToLower();
                    var price = decimal.Parse(item.Value.GetString() ?? "0");
                    Dispatcher.UIThread.Post(() =>
                        PriceUpdated?.Invoke(coinId, price));
                }
            }
            catch { }
        }

        public async Task DisconnectAsync()
        {
            if (_client != null)
            {
                await _client.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "");
                _client.Dispose();
                _client = null;
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
