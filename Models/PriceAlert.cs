using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAppCrypto.Models
{
    public partial class PriceAlert : ObservableObject
    {
        [ObservableProperty]
        private string _id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string _coinId = string.Empty;

        [ObservableProperty]
        private string _coinName = string.Empty;

        [ObservableProperty]
        private string _coinSymbol = string.Empty;

        [ObservableProperty]
        private string _coinImage = string.Empty;

        [ObservableProperty]
        private decimal _targetPrice;

        [ObservableProperty]
        private string _condition = "Above"; // "Above" or "Below"

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private string _notificationType = "Both"; // "App", "Sound", "Both"

        [ObservableProperty]
        private DateTime _dateCreated = DateTime.Now;

        [ObservableProperty]
        private decimal _currentPriceAtCreation;
    }
}
