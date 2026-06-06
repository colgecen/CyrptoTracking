using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAppCrypto.Models
{
    public partial class PortfolioEntry : ObservableObject
    {
        [ObservableProperty]
        private string _id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string _coinId = string.Empty;

        [ObservableProperty]
        private string _coinSymbol = string.Empty;

        [ObservableProperty]
        private string _coinName = string.Empty;

        [ObservableProperty]
        private string _coinImage = string.Empty;

        [ObservableProperty]
        private decimal _amount;

        [ObservableProperty]
        private decimal _averageBuyPrice;

        [ObservableProperty]
        private DateTime _dateAdded = DateTime.Now;
    }
}
