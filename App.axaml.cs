using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using AvaloniaAppCrypto.Interfaces;
using AvaloniaAppCrypto.Services;
using AvaloniaAppCrypto.ViewModels;
using AvaloniaAppCrypto.Views;

namespace AvaloniaAppCrypto
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IConnectivityService>(sp =>
            {
                var httpClient = new System.Net.Http.HttpClient();
                return new ConnectivityService(httpClient);
            });
            services.AddSingleton<ICoinGeckoService>(sp =>
            {
                var httpClient = new System.Net.Http.HttpClient();
                return new CoinGeckoService(httpClient, sp.GetRequiredService<ISettingsService>());
            });
            services.AddSingleton<IBinanceWsService, BinanceWsService>();
            services.AddSingleton<ICoinCapWsService, CoinCapWsService>();
            services.AddSingleton<ICryptoCompareService>(sp =>
            {
                var httpClient = new System.Net.Http.HttpClient();
                return new CryptoCompareService(httpClient, sp.GetRequiredService<ISettingsService>());
            });
            services.AddSingleton<IFavoriteService, FavoriteService>();
            services.AddSingleton<IPortfolioService, PortfolioService>();
            services.AddSingleton<IAlertService, AlertService>();
            services.AddSingleton<IToastService, ToastService>();

            // ViewModels
            services.AddTransient<MainWindowViewModel>();

            Services = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainVm = Services.GetRequiredService<MainWindowViewModel>();

                var mainWindow = new MainWindow
                {
                    DataContext = mainVm
                };
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                mainWindow.Activate();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
