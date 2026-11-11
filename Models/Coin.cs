using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAppCrypto.Models
{
    public partial class Coin : ObservableObject
    {
        [ObservableProperty]
        private string _id = string.Empty;

        [ObservableProperty]
        private string _symbol = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _image = string.Empty;

        [ObservableProperty]
        private decimal _currentPrice;

        [ObservableProperty]
        private long _marketCap;

        [ObservableProperty]
        private int _marketCapRank;

        [ObservableProperty]
        private decimal _totalVolume;

        [ObservableProperty]
        private decimal _high24H;

        [ObservableProperty]
        private decimal _low24H;

        [ObservableProperty]
        private double _priceChangePercentage24H;

        [ObservableProperty]
        private double _priceChangePercentage1H;

        [ObservableProperty]
        private double _priceChangePercentage7D;

        [ObservableProperty]
        private decimal _circulatingSupply;

        [ObservableProperty]
        private decimal? _totalSupply;

        [ObservableProperty]
        private decimal? _maxSupply;

        [ObservableProperty]
        private decimal _ath;

        [ObservableProperty]
        private string _athDate = string.Empty;

        [ObservableProperty]
        private decimal _atl;

        [ObservableProperty]
        private string _atlDate = string.Empty;

        [ObservableProperty]
        private List<double> _sparkline = new();

        [ObservableProperty]
        private bool _isFavorite;

        [ObservableProperty]
        private decimal _lastPrice;

        // 0: static, 1: up, -1: down
        [ObservableProperty]
        private int _priceDirection;

        [ObservableProperty]
        private IImage? _imageSource;
    }
}
