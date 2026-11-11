namespace AvaloniaAppCrypto.Models
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Dark"; // "Light", "Dark", "System"
        public string Language { get; set; } = "Türkçe"; // "Türkçe", "English"
        public string Currency { get; set; } = "USD"; // "USD", "EUR", "TRY"
        public bool IsCompactView { get; set; } = false;
        public int RefreshIntervalSeconds { get; set; } = 5;
        public bool AutoConnect { get; set; } = true;
        public int MaxCoinsCount { get; set; } = 50;
        public bool AlertsEnabled { get; set; } = true;
        public bool SoundEnabled { get; set; } = true;
        public int SoundVolume { get; set; } = 80;
        public string SelectedWebSocketSource { get; set; } = "Automatic"; // "Automatic", "Binance", "CoinCap"
        public string CoinGeckoApiKey { get; set; } = "CG-qwNsQdk56GiLdkg6VbwZWNXY";
        public string CryptoCompareApiKey { get; set; } = "178dfcdab7b6d31d62bec50e5819cd22aa2f5352a3b1b9dee5949bb928a70c60";
        public int ApiTimeoutSeconds { get; set; } = 10;
    }
}
