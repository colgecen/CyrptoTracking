using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using AvaloniaAppCrypto.ViewModels;

namespace AvaloniaAppCrypto.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;

        public MainWindow()
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0B0E14"));
            TransparencyLevelHint = new[] { WindowTransparencyLevel.None };
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;

            if (!ReferenceEquals(_viewModel, vm))
            {
                if (_viewModel != null)
                    _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

                _viewModel = vm;
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;

                PageHost.Content = CreatePage(_viewModel.CurrentPage);
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.Equals(e.PropertyName, nameof(MainWindowViewModel.CurrentPage), StringComparison.Ordinal))
                return;

            var page = CurrentPageFrom(_viewModel?.CurrentPage);
            PageHost.Content = CreatePage(page);
        }

        private static string CurrentPageFrom(string? page) => string.IsNullOrWhiteSpace(page) ? "Dashboard" : page;

        private Control CreatePage(string? page)
        {
            return CurrentPageFrom(page) switch
            {
                "Dashboard" => new DashboardView { DataContext = _viewModel },
                "Favorites" => new FavoritesView { DataContext = _viewModel },
                "Portfolio" => new PortfolioView { DataContext = _viewModel },
                "Alerts" => new AlertsView { DataContext = _viewModel },
                "News" => new NewsView { DataContext = _viewModel },
                "Settings" => new SettingsView { DataContext = _viewModel },
                _ => new DashboardView { DataContext = _viewModel }
            };
        }
    }
}
