using System;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaAppCrypto.Interfaces;
using Websocket.Client;

namespace AvaloniaAppCrypto.Services
{
    public class BinanceWsService : IBinanceWsService, IDisposable
    {
        private WebsocketClient? _client;
        private const string BinanceUrl = "wss://stream.binance.com:9443/stream?streams=";

        public event Action<string, decimal, decimal>? PriceUpdated;
        public bool IsConnected => _client?.IsRunning ?? false;

        public async Task ConnectAsync()
        {
            var symbols = new[] { "btcusdt", "ethusdt", "solusdt", "xrpusdt", "adausdt",
                "dogeusdt", "avaxusdt", "dotusdt", "linkusdt", "maticusdt",
                "shibusdt", "uniusdt", "atomusdt", "xlmusdt", "trousdt",
                "bchusdt", "ltcusdt", "algousdt", "vetusdt", "filusdt",
                "sandusdt", "manausdt", "aaveusdt", "axsusdt", "nearusdt",
                "ftmusdt", "crousdt", "apeusdt", "grtusdt", "renderusdt" };
            var streamNames = string.Join("/", Array.ConvertAll(symbols, s => $"{s}@miniTicker"));
            var url = BinanceUrl + streamNames;

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

                if (root.TryGetProperty("data", out var data))
                {
                    var symbol = data.GetProperty("s").GetString() ?? "";
                    var coinId = symbol.Replace("USDT", "").ToLower();
                    var price = decimal.Parse(data.GetProperty("c").GetString() ?? "0");
                    var changePct = decimal.Parse(data.GetProperty("P").GetString() ?? "0");

                    Dispatcher.UIThread.Post(() =>
                        PriceUpdated?.Invoke(coinId, price, changePct));
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
