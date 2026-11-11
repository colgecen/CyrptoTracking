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
        private readonly ICryptoCompareService _cryptoCompareService;
        private readonly IBinanceWsService _binanceWsService;
        private readonly ICoinCapWsService _coinCapWsService;
        private readonly IConnectivityService _connectivityService;
        private readonly IToastService _toastService;
        private readonly IFavoriteService _favoriteService;
        private readonly IAlertService _alertService;
        private readonly IPortfolioService _portfolioService;
        private readonly ISettingsService _settingsService;
        private AppSettings _settings = new();
        private DispatcherTimer? _autoRefreshTimer;
        private bool _isRefreshing;
        private DateTime _lastNewsRefresh = DateTime.MinValue;
        private DateTime _lastBtcChartRefresh = DateTime.MinValue;
        private DateTime _lastAlertCheck = DateTime.MinValue;
        private readonly Dictionary<string, DateTime> _lastWsUpdate = new(StringComparer.OrdinalIgnoreCase);

        [ObservableProperty] private bool _isSidebarCollapsed;
        [ObservableProperty] private string _currentPage = "Dashboard";
        [ObservableProperty] private bool _isConnected = true;
        [ObservableProperty] private string _connectionStatusText = "Bağlı";
        [ObservableProperty] private string _lastUpdated = "-";
        [ObservableProperty] private string _connectionStatusColor = "#10B981";
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private bool _isGridView;
        [ObservableProperty] private bool _isListView = true;

        // API keys (settings UI)
        [ObservableProperty] private string _coinGeckoApiKey = string.Empty;
        [ObservableProperty] private string _cryptoCompareApiKey = string.Empty;

        // Dashboard Global Data
        [ObservableProperty] private string _totalMarketCap = "$0";
        [ObservableProperty] private string _totalVolume24h = "$0";
        [ObservableProperty] private string _btcDominance = "0%";
        [ObservableProperty] private string _marketCapChange = "0%";
        [ObservableProperty] private string _fearGreedIndex = "72 (Açgözlük)";
        [ObservableProperty] private double _fearGreedValue = 50;

        // Chart Data
        [ObservableProperty]
        private List<double> _btcChartData = new();

        [ObservableProperty]
        private int _btcChartDays = 7;

        [ObservableProperty]
        private List<Models.OhlcPoint> _btcCandleData = new();

        private static int ParseDays(string? value)
        {
            if (int.TryParse(value, out var days) && days > 0)
                return days;
            return 0;
        }

        [RelayCommand]
        private async Task LoadBtcCandles(string? days)
        {
            var parsed = ParseDays(days);
            if (parsed <= 0) parsed = 7;
            days = null;
            try
            {
                var data = await _coinGeckoService.GetOhlcAsync("bitcoin", parsed);
                BtcCandleData = data;
            }
            catch (Exception ex)
            {
                _toastService.Show($"Mum grafiği yüklenemedi: {ex.Message}", ToastType.Error, 3000);
            }
        }

        [RelayCommand]
        private async Task LoadComparisonChart(string? days)
        {
            var parsed = ParseDays(days);
            if (parsed <= 0) parsed = 7;
            try
            {
                ComparisonSeries.Clear();
                var btcData = await _coinGeckoService.GetMarketChartAsync("bitcoin", parsed);
                var ethData = await _coinGeckoService.GetMarketChartAsync("ethereum", parsed);

                ComparisonSeries.Add(new Models.ChartSeries
                {
                    Name = "BTC",
                    Color = "#F59E0B",
                    Values = btcData.Select(d => Convert.ToDouble(d["price"])).ToList()
                });
                ComparisonSeries.Add(new Models.ChartSeries
                {
                    Name = "ETH",
                    Color = "#3B82F6",
                    Values = ethData.Select(d => Convert.ToDouble(d["price"])).ToList()
                });
            }
            catch (Exception ex)
            {
                _toastService.Show($"Karşılaştırma grafiği yüklenemedi: {ex.Message}", ToastType.Error, 3000);
            }
        }

        [RelayCommand]
        private async Task LoadBtcChart(string? days)
        {
            var parsed = ParseDays(days);
            BtcChartDays = parsed <= 0 ? 7 : parsed;
            try
            {
                var data = await _coinGeckoService.GetMarketChartAsync("bitcoin", BtcChartDays);
                var prices = data.Select(d => Convert.ToDouble(d["price"])).ToList();
                if (prices.Count > 0)
                    BtcChartData = prices;
            }
            catch (Exception ex)
            {
                _toastService.Show($"Grafik yüklenemedi: {ex.Message}", ToastType.Error, 3000);
            }
        }

        // Favorited Coins (filtered)
        public ObservableCollection<Coin> FavoritedCoins { get; } = new();

        public ObservableCollection<Models.NavigationItem> NavigationItems { get; } = new()
        {
            new Models.NavigationItem { Page = "Dashboard", Icon = "📊", Title = "Dashboard", IsActive = true },
            new Models.NavigationItem { Page = "Favorites", Icon = "⭐", Title = "Favoriler" },
            new Models.NavigationItem { Page = "Portfolio", Icon = "💼", Title = "Portföy" },
            new Models.NavigationItem { Page = "Alerts", Icon = "🔔", Title = "Alarmlar" },
            new Models.NavigationItem { Page = "News", Icon = "📰", Title = "Haberler" },
            new Models.NavigationItem { Page = "Settings", Icon = "⚙️", Title = "Ayarlar" }
        };

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
        public ObservableCollection<Models.ChartSegment> PortfolioSegments { get; } = new();
        public ObservableCollection<Models.ChartSegment> MarketSegments { get; } = new();
        public ObservableCollection<Models.ChartSegment> TopGainers { get; } = new();
        public ObservableCollection<Models.ChartSeries> ComparisonSeries { get; } = new();
        public ObservableCollection<PriceAlert> Alerts { get; } = new();
        public ObservableCollection<PriceAlert> TriggeredAlerts { get; } = new();
        public ObservableCollection<NewsArticle> NewsArticles { get; } = new();

        // Portfolio add form
        [ObservableProperty] private bool _isAddPortfolioOpen;
        [ObservableProperty] private Coin? _newPortfolioCoin;
        [ObservableProperty] private decimal _newPortfolioAmount;
        [ObservableProperty] private decimal _newPortfolioBuyPrice;

        // Portfolio summary
        [ObservableProperty] private string _portfolioTotalValue = "$0";
        [ObservableProperty] private string _portfolioProfitLoss = "$0";
        [ObservableProperty] private string _portfolioProfitLossPercent = "0%";

        // News
        [ObservableProperty] private string _newsLastUpdate = "";

        // Alert filter
        [ObservableProperty] private string _alertFilter = "Tümü";

        public System.Collections.ObjectModel.ObservableCollection<string> AlertConditions { get; } = new() { "Above", "Below" };

        // New alert form
        [ObservableProperty] private bool _isAddAlertOpen;
        [ObservableProperty] private Coin? _newAlertCoin;
        [ObservableProperty] private decimal _newAlertTargetPrice;
        [ObservableProperty] private string _newAlertCondition = "Above";

        public MainWindowViewModel(
            ICoinGeckoService coinGeckoService,
            ICryptoCompareService cryptoCompareService,
            IBinanceWsService binanceWsService,
            ICoinCapWsService coinCapWsService,
            IConnectivityService connectivityService,
            IToastService toastService,
            IFavoriteService favoriteService,
            IAlertService alertService,
            IPortfolioService portfolioService,
            ISettingsService settingsService)
        {
            _coinGeckoService = coinGeckoService;
            _cryptoCompareService = cryptoCompareService;
            _binanceWsService = binanceWsService;
            _coinCapWsService = coinCapWsService;
            _connectivityService = connectivityService;
            _toastService = toastService;
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
            CoinGeckoApiKey = _settings.CoinGeckoApiKey ?? string.Empty;
            CryptoCompareApiKey = _settings.CryptoCompareApiKey ?? string.Empty;

            if (string.IsNullOrWhiteSpace(CoinGeckoApiKey) || string.IsNullOrWhiteSpace(CryptoCompareApiKey))
                _toastService.Show("API anahtarı eksik. Ayarlar ekranından girin.", ToastType.Warning, 0);

            StartAutoRefresh();

            var favs = await _favoriteService.GetFavoritesAsync();
            foreach (var f in favs) FavoriteIds.Add(f);

            await LoadCoinsAsync();
            await LoadGlobalDataAsync();
            await LoadPortfolioAsync();
            await LoadAlertsAsync();
            await LoadNewsAsync();
            await LoadBtcCandles("7");
            await LoadComparisonChart("7");
            LastUpdated = DateTime.Now.ToString("HH:mm:ss");
            _ = ConnectWebSocketsAsync();
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

                var (fgValue, fgLabel) = await _coinGeckoService.GetFearGreedIndexAsync();
                if (fgValue > 0)
                {
                    FearGreedIndex = $"{fgValue} ({fgLabel})";
                    FearGreedValue = fgValue;
                }
            }
            catch { }
        }

        // update fear greed inside LoadGlobalData? We'll add after market data block maybe. Actually LoadGlobalDataAsync already above. Let's patch separately.
        public async Task LoadPortfolioAsync()
        {
            try
            {
                var entries = await _portfolioService.GetPortfolioAsync();
                PortfolioEntries.Clear();
                PortfolioSegments.Clear();
                decimal totalVal = 0, totalCost = 0;
                var palette = new[] { "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6", "#EC4899", "#14B8A6" };
                int colorIndex = 0;
                foreach (var entry in entries)
                {
                    var coin = Coins.FirstOrDefault(c => c.Id == entry.CoinId);
                    var currentPrice = coin?.CurrentPrice ?? 0;
                    var value = entry.Amount * currentPrice;
                    var cost = entry.Amount * entry.AverageBuyPrice;
                    totalVal += value; totalCost += cost;
                    PortfolioEntries.Add(entry);
                    PortfolioSegments.Add(new Models.ChartSegment
                    {
                        Label = entry.CoinSymbol,
                        Value = (double)value,
                        Color = palette[colorIndex % palette.Length]
                    });
                    colorIndex++;
                }
                var pnl = totalVal - totalCost;
                PortfolioTotalValue = $"${totalVal:N2}";
                PortfolioProfitLoss = pnl >= 0 ? $"+${pnl:N2}" : $"-${Math.Abs(pnl):N2}";
                PortfolioProfitLossPercent = totalCost > 0 ? $"{(pnl / totalCost) * 100:F2}%" : "0%";
            }
            catch { }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await RefreshDataAsync(true);
        }

        private async Task RefreshDataAsync(bool showToast = false)
        {
            // Auto-refresh can fire again before the previous round finished;
            // overlapping full reloads hammer the API and make the UI stutter.
            if (_isRefreshing) return;
            _isRefreshing = true;
            try
            {
                await LoadCoinsAsync();
                await LoadGlobalDataAsync();
                await LoadPortfolioAsync();
                await LoadAlertsAsync();

                // News changes slowly; refreshing it on every price tick only
                // adds latency and rebuilds ~100 list rows for nothing.
                if (showToast || (DateTime.UtcNow - _lastNewsRefresh) > TimeSpan.FromMinutes(5))
                    await LoadNewsAsync();

                if (showToast)
                    _ = LoadComparisonChart(BtcChartDays.ToString());

                LastUpdated = DateTime.Now.ToString("HH:mm:ss");
                if (showToast)
                    _toastService.Show("Veriler yenilendi", ToastType.Success, 2000);
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task LoadCoinsAsync()
        {
            try
            {
                var coins = await _coinGeckoService.GetTopCoinsAsync(50);

                // Update coins in place so the list is not torn down and
                // rebuilt on every refresh (rebuilding 50 rows ~every tick is
                // what made the UI stutter and lose scroll position).
                var existing = Coins.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
                var incoming = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var listChanged = false;

                for (var i = 0; i < coins.Count; i++)
                {
                    var fresh = coins[i];
                    incoming.Add(fresh.Id);
                    if (existing.TryGetValue(fresh.Id, out var target))
                    {
                        target.Symbol = fresh.Symbol;
                        target.Name = fresh.Name;
                        target.Image = fresh.Image;
                        target.CurrentPrice = fresh.CurrentPrice;
                        target.MarketCap = fresh.MarketCap;
                        target.MarketCapRank = fresh.MarketCapRank;
                        target.TotalVolume = fresh.TotalVolume;
                        target.High24H = fresh.High24H;
                        target.Low24H = fresh.Low24H;
                        target.PriceChangePercentage24H = fresh.PriceChangePercentage24H;
                        target.PriceChangePercentage1H = fresh.PriceChangePercentage1H;
                        target.PriceChangePercentage7D = fresh.PriceChangePercentage7D;
                        target.CirculatingSupply = fresh.CirculatingSupply;
                        target.TotalSupply = fresh.TotalSupply;
                        target.MaxSupply = fresh.MaxSupply;
                        target.Ath = fresh.Ath;
                        target.AthDate = fresh.AthDate;
                        target.IsFavorite = FavoriteIds.Contains(fresh.Id);
                    }
                    else
                    {
                        fresh.IsFavorite = FavoriteIds.Contains(fresh.Id);
                        Coins.Add(fresh);
                        listChanged = true;
                    }
                }

                foreach (var stale in Coins.Where(c => !incoming.Contains(c.Id)).ToList())
                {
                    Coins.Remove(stale);
                    listChanged = true;
                }

                // Always refresh derived views: prices/percentages change even
                // when the coin set itself hasn't changed.
                // Kick off icon downloads in parallel; images are cached and
                // setting the same cached bitmap is a no-op for bindings.
                _ = LoadImageSourcesAsync();

                if (listChanged)
                {
                    ApplyFilter();
                    RefreshFavorites();
                }
                BuildMarketSegments();
                BuildTopGainers();

                // Refresh the sparkline periodically or on structural change.
                if (listChanged || (DateTime.UtcNow - _lastBtcChartRefresh) > TimeSpan.FromMinutes(5))
                    UpdateBtcChart();
            }
            catch (Exception ex)
            {
                _toastService.Show($"Veri yüklenemedi: {ex.Message}", ToastType.Error);
            }
        }

        private void UpdateBtcChart()
        {
            _lastBtcChartRefresh = DateTime.UtcNow;
            _ = LoadBtcChart(BtcChartDays.ToString());
        }
        private async Task LoadImageSourcesAsync()
        {
            var coins = Coins.ToList();
            var tasks = coins.Select(async coin =>
            {
                var img = await Services.ImageLoader.GetAsync(coin.Image).ConfigureAwait(false);
                if (img is not null && !ReferenceEquals(coin.ImageSource, img))
                    coin.ImageSource = img;
            });
            await Task.WhenAll(tasks);
        }



        private void BuildMarketSegments()
        {
            MarketSegments.Clear();
            var palette = new[] { "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6", "#EC4899", "#14B8A6", "#64748B" };
            var top = Coins.OrderByDescending(c => c.MarketCap).Take(6).ToList();
            var other = Coins.Skip(6).Sum(c => (double)c.MarketCap);
            int i = 0;
            foreach (var c in top)
            {
                MarketSegments.Add(new Models.ChartSegment
                {
                    Label = c.Symbol.ToUpper(),
                    Value = (double)c.MarketCap,
                    Color = palette[i % palette.Length]
                });
                i++;
            }
            if (other > 0)
            {
                MarketSegments.Add(new Models.ChartSegment
                {
                    Label = "Diğer",
                    Value = other,
                    Color = "#64748B"
                });
            }
        }

        private void BuildTopGainers()
        {
            TopGainers.Clear();
            var palette = new[] { "#10B981", "#22C55E", "#34D399", "#6EE7B7", "#A7F3D0" };
            var top = Coins.OrderByDescending(c => c.PriceChangePercentage24H).Take(5).ToList();
            int i = 0;
            foreach (var c in top)
            {
                TopGainers.Add(new Models.ChartSegment
                {
                    Label = c.Symbol.ToUpper(),
                    Value = c.PriceChangePercentage24H,
                    Color = palette[i % palette.Length]
                });
                i++;
            }
        }

        [ObservableProperty] private string _sortColumn = "Rank";
        [ObservableProperty] private bool _sortAscending = true;

        [RelayCommand]
        private void SortCoins(string column)
        {
            if (SortColumn == column)
                SortAscending = !SortAscending;
            else
            {
                SortColumn = column;
                SortAscending = true;
            }
            ApplyFilter();
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            var query = SearchQuery?.Trim().ToLowerInvariant() ?? string.Empty;
            bool matches(string? value) => value is not null &&
                                           value.Contains(query, StringComparison.OrdinalIgnoreCase);

            var filtered = string.IsNullOrEmpty(query)
                ? Coins.ToList()
                : Coins.Where(c =>
                        matches(c.Name) ||
                        matches(c.Symbol))
                    .ToList();

            IOrderedEnumerable<Coin> sorted = SortColumn switch
            {
                "Name" => SortAscending ? filtered.OrderBy(c => c.Name) : filtered.OrderByDescending(c => c.Name),
                "Price" => SortAscending ? filtered.OrderBy(c => c.CurrentPrice) : filtered.OrderByDescending(c => c.CurrentPrice),
                "Change1h" => SortAscending ? filtered.OrderBy(c => c.PriceChangePercentage1H) : filtered.OrderByDescending(c => c.PriceChangePercentage1H),
                "Change24h" => SortAscending ? filtered.OrderBy(c => c.PriceChangePercentage24H) : filtered.OrderByDescending(c => c.PriceChangePercentage24H),
                "Change7d" => SortAscending ? filtered.OrderBy(c => c.PriceChangePercentage7D) : filtered.OrderByDescending(c => c.PriceChangePercentage7D),
                "Volume" => SortAscending ? filtered.OrderBy(c => c.TotalVolume) : filtered.OrderByDescending(c => c.TotalVolume),
                "MarketCap" => SortAscending ? filtered.OrderBy(c => c.MarketCap) : filtered.OrderByDescending(c => c.MarketCap),
                _ => SortAscending ? filtered.OrderBy(c => c.MarketCapRank) : filtered.OrderByDescending(c => c.MarketCapRank)
            };

            FilteredCoins.Clear();
            foreach (var coin in sorted)
                FilteredCoins.Add(coin);
        }

        private DispatcherTimer? _searchDebounceTimer;

        partial void OnSearchQueryChanged(string value)
        {
            _searchDebounceTimer?.Stop();
            _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchDebounceTimer.Tick += (_, _) =>
            {
                _searchDebounceTimer?.Stop();
                ApplyFilter();
            };
            _searchDebounceTimer.Start();
        }

        partial void OnNewAlertCoinChanged(Coin? value)
        {
            if (value == null) return;
            NewAlertTargetPrice = value.CurrentPrice;
        }

        partial void OnNewPortfolioCoinChanged(Coin? value)
        {
            if (value == null) return;
            NewPortfolioBuyPrice = value.CurrentPrice;
        }

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
        private void NavigateTo(string page)
        {
            CurrentPage = page;
            foreach (var item in NavigationItems)
                item.IsActive = string.Equals(item.Page, page, StringComparison.Ordinal);
        }

        [RelayCommand]
        private void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

        [RelayCommand]
        private void SetCoinViewMode(string? mode)
        {
            IsGridView = string.Equals(mode, "Grid", StringComparison.OrdinalIgnoreCase);
            IsListView = !IsGridView;
        }

        [RelayCommand]
        private async Task SaveSettings()
        {
            _settings.CoinGeckoApiKey = CoinGeckoApiKey;
            _settings.CryptoCompareApiKey = CryptoCompareApiKey;
            await _settingsService.SaveSettingsAsync(_settings);
            StartAutoRefresh();
            _toastService.Show("Ayarlar kaydedildi", ToastType.Success, 3000);
        }

        private void StartAutoRefresh()
        {
            _autoRefreshTimer?.Stop();
            var seconds = _settings.RefreshIntervalSeconds;
            if (seconds <= 0) return; // 0 means auto-refresh disabled
            // Respect user setting but keep a small floor to avoid hammering the API.
            seconds = Math.Max(seconds, 5);
            _autoRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
            _autoRefreshTimer.Tick += async (_, _) => await RefreshDataAsync(false);
            _autoRefreshTimer.Start();
        }

        [RelayCommand]
        private async Task TestApiKeys()
        {
            try
            {
                await _coinGeckoService.GetTopCoinsAsync(1);
                _toastService.Show("CoinGecko API anahtarı geçerli", ToastType.Success, 3000);
            }
            catch (Exception ex)
            {
                _toastService.Show($"CoinGecko API hatası: {ex.Message}", ToastType.Error, 0);
            }
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
            var coin = Coins.FirstOrDefault(c =>
                string.Equals(c.Id, coinId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Symbol, coinId, StringComparison.OrdinalIgnoreCase));
            if (coin == null) return;

            // Throttle UI updates to reduce stutter from high-frequency ticks.
            var now = DateTime.UtcNow;
            if (_lastWsUpdate.TryGetValue(coin.Id, out var last) && (now - last).TotalMilliseconds < 500)
                return;
            _lastWsUpdate[coin.Id] = now;

            coin.PriceDirection = price > coin.CurrentPrice ? 1 : price < coin.CurrentPrice ? -1 : 0;
            coin.CurrentPrice = price;
            coin.PriceChangePercentage24H = (double)changePct;
            LastUpdated = DateTime.Now.ToString("HH:mm:ss");
            _ = CheckPriceAlertsAsync();
        }

        private async Task CheckPriceAlertsAsync()
        {
            if ((DateTime.Now - _lastAlertCheck).TotalSeconds < 5) return;
            _lastAlertCheck = DateTime.Now;
            try
            {
                var prices = Coins.ToDictionary(c => c.Id.ToLower(), c => c.CurrentPrice);
                var triggered = await _alertService.CheckAlertsAsync(prices);
                if (triggered.Count > 0)
                {
                    foreach (var t in triggered)
                        _toastService.Show($"Alarm tetiklendi: {t.CoinSymbol} {t.Condition} {t.TargetPrice:C2}", ToastType.Warning, 0);
                    await LoadAlertsAsync();
                }
            }
            catch { }
        }

        private void OnCoinCapPriceUpdated(string coinId, decimal price)
        {
            var coin = Coins.FirstOrDefault(c =>
                string.Equals(c.Id, coinId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Symbol, coinId, StringComparison.OrdinalIgnoreCase));
            if (coin == null) return;

            var now = DateTime.UtcNow;
            if (_lastWsUpdate.TryGetValue(coin.Id, out var last) && (now - last).TotalMilliseconds < 500)
                return;
            _lastWsUpdate[coin.Id] = now;

            coin.PriceDirection = price > coin.CurrentPrice ? 1 : price < coin.CurrentPrice ? -1 : 0;
            coin.CurrentPrice = price;
            LastUpdated = DateTime.Now.ToString("HH:mm:ss");
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

        private async Task LoadNewsAsync()
        {
            try
            {
                var articles = await _cryptoCompareService.GetLatestNewsAsync();
                NewsArticles.Clear();
                foreach (var article in articles)
                    NewsArticles.Add(article);
                NewsLastUpdate = DateTime.Now.ToString("HH:mm");
                _lastNewsRefresh = DateTime.UtcNow;
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

        [RelayCommand]
        private void OpenAddPortfolio() => IsAddPortfolioOpen = true;

        [RelayCommand]
        private void CloseAddPortfolio() => IsAddPortfolioOpen = false;

        [RelayCommand]
        private async Task AddPortfolioEntry()
        {
            if (NewPortfolioCoin == null) { _toastService.Show("Coin seçin", ToastType.Error, 2000); return; }
            if (NewPortfolioAmount <= 0) { _toastService.Show("Miktar girin", ToastType.Error, 2000); return; }
            var entry = new PortfolioEntry
            {
                CoinId = NewPortfolioCoin.Id,
                CoinSymbol = NewPortfolioCoin.Symbol,
                CoinName = NewPortfolioCoin.Name,
                CoinImage = NewPortfolioCoin.Image,
                Amount = NewPortfolioAmount,
                AverageBuyPrice = NewPortfolioBuyPrice > 0 ? NewPortfolioBuyPrice : NewPortfolioCoin.CurrentPrice
            };
            await _portfolioService.AddEntryAsync(entry);
            await LoadPortfolioAsync();
            IsAddPortfolioOpen = false;
            _toastService.Show("Portföy girişi eklendi", ToastType.Success, 2000);
        }

        // Alert commands
        [RelayCommand]
        private async Task RemoveAlert(string id) { await _alertService.RemoveAlertAsync(id); await LoadAlertsAsync(); }

        [RelayCommand]
        private void OpenAddAlert() => IsAddAlertOpen = true;

        [RelayCommand]
        private void CloseAddAlert() => IsAddAlertOpen = false;

        [RelayCommand]
        private async Task AddAlert()
        {
            if (NewAlertCoin == null) { _toastService.Show("Coin seçin", ToastType.Error, 2000); return; }
            if (NewAlertTargetPrice <= 0) { _toastService.Show("Hedef fiyat girin", ToastType.Error, 2000); return; }
            var alert = new PriceAlert
            {
                CoinId = NewAlertCoin.Id,
                CoinName = NewAlertCoin.Name,
                CoinSymbol = NewAlertCoin.Symbol,
                CoinImage = NewAlertCoin.Image,
                TargetPrice = NewAlertTargetPrice,
                Condition = NewAlertCondition,
                CurrentPriceAtCreation = NewAlertCoin.CurrentPrice,
                IsActive = true
            };
            await _alertService.AddAlertAsync(alert);
            await LoadAlertsAsync();
            IsAddAlertOpen = false;
            _toastService.Show("Alarm eklendi", ToastType.Success, 2000);
        }

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
