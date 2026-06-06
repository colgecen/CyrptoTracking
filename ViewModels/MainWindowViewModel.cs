using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAppCrypto.Interfaces;
using AvaloniaAppCrypto.Models;


namespace AvaloniaAppCrypto.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ICoinGeckoService _coinGeckoService;
        private readonly IBinanceWsService _binanceWsService;
        private readonly ICoinCapWsService _coinCapWsService;
        private readonly IConnectivityService _connectivityService;
        private readonly IToastService _toastService;
        private readonly IThemeService _themeService;
        private readonly IFavoriteService _favoriteService;
        private readonly IAlertService _alertService;
        private readonly IPortfolioService _portfolioService;
        private readonly ISettingsService _settingsService;
        private AppSettings _settings = new();

        [ObservableProperty] private bool _isSidebarCollapsed;
        [ObservableProperty] private string _currentPage = "Dashboard";
        [ObservableProperty] private bool _isConnected = true;
        [ObservableProperty] private string _connectionStatusText = "Bağlı";
        [ObservableProperty] private string _connectionStatusColor = "#10B981";
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private string _currentThemeIcon = "🌙";

        // Dashboard Global Data
        [ObservableProperty] private string _totalMarketCap = "$0";
        [ObservableProperty] private string _totalVolume24h = "$0";
        [ObservableProperty] private string _btcDominance = "0%";
        [ObservableProperty] private string _marketCapChange = "0%";
        [ObservableProperty] private string _fearGreedIndex = "72 (Açgözlük)";

        // Chart Data
        [ObservableProperty]
        private List<double> _btcChartData = new();

        // Favorited Coins (filtered)
        public ObservableCollection<Coin> FavoritedCoins { get; } = new();

        private void RefreshFavorites()
        {
            FavoritedCoins.Clear();
            foreach (var c in Coins.Where(c => c.IsFavorite))
                FavoritedCoins.Add(c);
        }

        // Favorites
        public ObservableCollection<string> FavoriteIds { get; } = new();

        // Coin Collections
        public ObservableCollection<Coin> Coins { get; } = new();
        public ObservableCollection<Coin> FilteredCoins { get; } = new();

        // Portfolio
        public ObservableCollection<PortfolioEntry> PortfolioEntries { get; } = new();
        public ObservableCollection<PriceAlert> Alerts { get; } = new();
        public ObservableCollection<PriceAlert> TriggeredAlerts { get; } = new();
        public ObservableCollection<NewsArticle> NewsArticles { get; } = new();

        // Portfolio summary
        [ObservableProperty] private string _portfolioTotalValue = "$0";
        [ObservableProperty] private string _portfolioProfitLoss = "$0";
        [ObservableProperty] private string _portfolioProfitLossPercent = "0%";

        // News
        [ObservableProperty] private string _newsLastUpdate = "";

        // Alert filter
        [ObservableProperty] private string _alertFilter = "Tümü";

        public MainWindowViewModel(
            ICoinGeckoService coinGeckoService,
            IBinanceWsService binanceWsService,
            ICoinCapWsService coinCapWsService,
            IConnectivityService connectivityService,
            IToastService toastService,
            IThemeService themeService,
            IFavoriteService favoriteService,
            IAlertService alertService,
            IPortfolioService portfolioService,
            ISettingsService settingsService)
        {
            _coinGeckoService = coinGeckoService;
            _binanceWsService = binanceWsService;
            _coinCapWsService = coinCapWsService;
            _connectivityService = connectivityService;
            _toastService = toastService;
            _themeService = themeService;
            _favoriteService = favoriteService;
            _alertService = alertService;
            _portfolioService = portfolioService;
            _settingsService = settingsService;

            _connectivityService.ConnectivityChanged += OnConnectivityChanged;
            _binanceWsService.PriceUpdated += OnPriceUpdated;
            _coinCapWsService.PriceUpdated += OnCoinCapPriceUpdated;

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            _settings = await _settingsService.GetSettingsAsync();
            _themeService.SetTheme(_settings.Theme);
            CurrentThemeIcon = _settings.Theme == "Light" ? "☀️" : "🌙";

            var favs = await _favoriteService.GetFavoritesAsync();
            foreach (var f in favs) FavoriteIds.Add(f);

            await LoadCoinsAsync();
            await LoadGlobalDataAsync();
            await LoadPortfolioAsync();
            await LoadAlertsAsync();
            await ConnectWebSocketsAsync();
        }

        private async Task LoadGlobalDataAsync()
        {
            try
            {
                var data = await _coinGeckoService.GetGlobalDataAsync();
                if (data.TryGetValue("total_market_cap", out var tmc))
                    TotalMarketCap = FormatLargeNumber(Convert.ToDecimal(tmc));
                if (data.TryGetValue("total_volume", out var tv))
                    TotalVolume24h = FormatLargeNumber(Convert.ToDecimal(tv));
                if (data.TryGetValue("btc_dominance", out var btc))
                    BtcDominance = $"{Convert.ToDecimal(btc):F1}%";
                if (data.TryGetValue("market_cap_change_24h", out var chg))
                    MarketCapChange = $"{Convert.ToDecimal(chg):F1}%";
            }
            catch { }
        }

        public async Task LoadPortfolioAsync()
        {
            try
            {
                var entries = await _portfolioService.GetPortfolioAsync();
                PortfolioEntries.Clear();
                decimal totalVal = 0, totalCost = 0;
                foreach (var entry in entries)
                {
                    var coin = Coins.FirstOrDefault(c => c.Id == entry.CoinId);
                    var currentPrice = coin?.CurrentPrice ?? 0;
                    var value = entry.Amount * currentPrice;
                    var cost = entry.Amount * entry.AverageBuyPrice;
                    totalVal += value; totalCost += cost;
                    PortfolioEntries.Add(entry);
                }
                var pnl = totalVal - totalCost;
                PortfolioTotalValue = $"${totalVal:N2}";
                PortfolioProfitLoss = pnl >= 0 ? $"+${pnl:N2}" : $"-${Math.Abs(pnl):N2}";
                PortfolioProfitLossPercent = totalCost > 0 ? $"{(pnl / totalCost) * 100:F2}%" : "0%";
            }
            catch { }
        }

        [RelayCommand]
        private async Task LoadCoinsAsync()
        {
            try
            {
                var coins = await _coinGeckoService.GetTopCoinsAsync(50);
                Coins.Clear();
                foreach (var coin in coins)
                {
                    coin.IsFavorite = FavoriteIds.Contains(coin.Id);
                    Coins.Add(coin);
                }
                ApplyFilter();
                RefreshFavorites();
                UpdateBtcChart();
            }
            catch (Exception ex)
            {
                _toastService.Show($"Veri yüklenemedi: {ex.Message}", ToastType.Error);
            }
        }

        private void UpdateBtcChart()
        {
            var btc = Coins.FirstOrDefault(c => c.Id == "bitcoin");
            if (btc?.Sparkline != null && btc.Sparkline.Count > 0)
            {
                BtcChartData = new List<double>(btc.Sparkline);
            }
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            var query = SearchQuery?.ToLower() ?? "";
            FilteredCoins.Clear();
            var filtered = Coins.Where(c =>
                string.IsNullOrEmpty(query) ||
                c.Name.ToLower().Contains(query) ||
                c.Symbol.ToLower().Contains(query));
            foreach (var coin in filtered)
                FilteredCoins.Add(coin);
        }

        partial void OnSearchQueryChanged(string value) => ApplyFilter();

        [RelayCommand]
        private async Task ToggleFavorite(string coinId)
        {
            var coin = Coins.FirstOrDefault(c => c.Id == coinId);
            if (coin == null) return;
            coin.IsFavorite = !coin.IsFavorite;
            if (coin.IsFavorite)
            {
                await _favoriteService.AddFavoriteAsync(coinId);
                if (!FavoriteIds.Contains(coinId)) FavoriteIds.Add(coinId);
            }
            else
            {
                await _favoriteService.RemoveFavoriteAsync(coinId);
                FavoriteIds.Remove(coinId);
            }
            RefreshFavorites();
        }

        [RelayCommand]
        private void NavigateTo(string page) => CurrentPage = page;

        [RelayCommand]
        private void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

        [RelayCommand]
        private void ToggleTheme()
        {
            var newTheme = _settings.Theme == "Light" ? "Dark" : "Light";
            _settings.Theme = newTheme;
            _themeService.SetTheme(newTheme);
            CurrentThemeIcon = newTheme == "Light" ? "☀️" : "🌙";
            _ = _settingsService.SaveSettingsAsync(_settings);
        }

        private async Task ConnectWebSocketsAsync()
        {
            try
            {
                await _binanceWsService.ConnectAsync();
                ConnectionStatusText = "Bağlı"; ConnectionStatusColor = "#10B981"; IsConnected = true;
            }
            catch
            {
                try
                {
                    var assets = Coins.Select(c => c.Id).ToList();
                    await _coinCapWsService.ConnectAsync(assets);
                    ConnectionStatusText = "CoinCap (Yedek)"; ConnectionStatusColor = "#F59E0B"; IsConnected = true;
                }
                catch
                {
                    ConnectionStatusText = "Bağlantı Yok"; ConnectionStatusColor = "#EF4444"; IsConnected = false;
                    _toastService.Show("WebSocket bağlantısı kurulamadı", ToastType.Error, 0);
                }
            }
        }

        private void OnPriceUpdated(string coinId, decimal price, decimal changePct)
        {
            var coin = Coins.FirstOrDefault(c => c.Id == coinId);
            if (coin == null) return;
            coin.PriceDirection = price > coin.CurrentPrice ? 1 : price < coin.CurrentPrice ? -1 : 0;
            coin.CurrentPrice = price;
            coin.PriceChangePercentage24H = (double)changePct;
        }

        private void OnCoinCapPriceUpdated(string coinId, decimal price)
        {
            var coin = Coins.FirstOrDefault(c => c.Id == coinId);
            if (coin == null) return;
            coin.PriceDirection = price > coin.CurrentPrice ? 1 : price < coin.CurrentPrice ? -1 : 0;
            coin.CurrentPrice = price;
        }

        private void OnConnectivityChanged(bool isOnline)
        {
            IsConnected = isOnline;
            if (!isOnline)
            {
                ConnectionStatusText = "İnternet Yok"; ConnectionStatusColor = "#EF4444";
                _toastService.Show("İnternet bağlantısı kesildi", ToastType.Error, 0);
            }
            else
            {
                ConnectionStatusText = "Bağlı"; ConnectionStatusColor = "#10B981";
                _toastService.Show("Bağlantı yenilendi", ToastType.Success, 3000);
            }
        }

        private async Task LoadAlertsAsync()
        {
            try
            {
                var all = await _alertService.GetAlertsAsync();
                Alerts.Clear(); TriggeredAlerts.Clear();
                foreach (var a in all)
                {
                    if (a.IsActive) Alerts.Add(a); else TriggeredAlerts.Add(a);
                }
            }
            catch { }
        }

        // Portfolio commands
        [RelayCommand]
        private async Task RemovePortfolioEntry(string id)
        {
            await _portfolioService.RemoveEntryAsync(id);
            await LoadPortfolioAsync();
        }

        // Alert commands
        [RelayCommand]
        private async Task RemoveAlert(string id) { await _alertService.RemoveAlertAsync(id); await LoadAlertsAsync(); }

        [RelayCommand]
        private async Task ToggleAlert(PriceAlert alert)
        {
            alert.IsActive = !alert.IsActive;
            await _alertService.UpdateAlertAsync(alert);
            await LoadAlertsAsync();
        }

        [RelayCommand]
        private async Task ClearTriggered() { await _alertService.ClearTriggeredAsync(); TriggeredAlerts.Clear(); }

        private static string FormatLargeNumber(decimal value)
        {
            if (Math.Abs(value) >= 1_000_000_000_000) return $"${value / 1_000_000_000_000m:F2}T";
            if (Math.Abs(value) >= 1_000_000_000) return $"${value / 1_000_000_000m:F2}B";
            if (Math.Abs(value) >= 1_000_000) return $"${value / 1_000_000m:F2}M";
            if (Math.Abs(value) >= 1_000) return $"${value / 1_000m:F2}K";
            return $"${value:F2}";
        }
    }
}
