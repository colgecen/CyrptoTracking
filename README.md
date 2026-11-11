# CryptoTrack

> Cross-platform real-time crypto market tracking & portfolio manager

A cross-platform desktop application built with **Avalonia UI** and **.NET 8** for tracking cryptocurrency markets, managing a personal portfolio, setting price alerts, and staying up-to-date with crypto news.

![Avalonia](https://img.shields.io/badge/Avalonia-12.0.3-purple)
![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)

---

## Features

- **Live Dashboard** — Real-time prices, market cap, volume, BTC dominance, and fear & greed index
- **BTC Chart** — 7-day price sparkline with green/red trend indicators
- **Coin Table** — Sortable list of top 50 coins with 1h / 24h / 7d changes
- **Favorites** — Star coins to track them separately
- **Portfolio** — Track your holdings with P/L calculations
- **Price Alerts** — Set and manage price threshold notifications
- **Crypto News** — Browse latest articles from CryptoCompare
- **Real-time WebSocket** — Live price updates via Binance (primary) or CoinCap (fallback)
- **Dark / Light Theme** — Switchable UI themes
- **Multi-currency** — USD, EUR, TRY support

## Screenshots

_Coming soon_

## Tech Stack

| Layer       | Technology |
|-------------|-----------|
| UI Framework | Avalonia UI 12 |
| Language     | C# 12 |
| Runtime      | .NET 8 |
| Architecture | MVVM + CommunityToolkit.Mvvm |
| DI           | Microsoft.Extensions.DependencyInjection |
| REST API     | CoinGecko |
| WebSocket    | Binance / CoinCap |
| News API     | CryptoCompare |
| Charting     | Custom SkiaSharp-based SparklineChart |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

Linux (Fedora/Ubuntu) için ek paketler:
```bash
# Fedora
sudo dnf install libX11 libICE libSM fontconfig
# Ubuntu/Debian
sudo apt install libx11-6 libice6 libsm6 fontconfig
```

Linux'ta .NET 8 runtime yoksa:
```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
export PATH="$HOME/.dotnet:$PATH"
```

### Run

```bash
cd AvaloniaAppCrypto
dotnet run --project AvaloniaAppCrypto
```

### Build

```bash
dotnet build
```

## Project Structure

```
AvaloniaAppCrypto/
├── Controls/          # Custom UI controls (e.g. SparklineChart)
├── Converters/        # XAML value converters
├── Interfaces/        # Service interfaces
├── Models/            # Data models (Coin, PortfolioEntry, etc.)
├── Services/          # API, WebSocket, storage services
├── Styles/            # Theme dictionaries (Dark, Light) + base styles
├── ViewModels/        # MVVM view models
├── Views/             # XAML views
├── App.axaml          # Application resources & styles
└── App.axaml.cs       # DI container setup
```

## Configuration

API keys and service endpoints are configured in the source code:

- **CoinGecko** — Uses a free-tier API key
- **CryptoCompare** — Uses a free-tier API key
- **Binance WebSocket** — Public endpoint, no key required
- **CoinCap WebSocket** — Public endpoint, no key required

Data persistence is stored in `%AppData%/CryptoTrack/` as JSON files.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
