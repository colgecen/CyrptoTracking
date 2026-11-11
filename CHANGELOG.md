# Changelog

## [Unreleased]
### Fixed
- **Critical: window rendered blank/transparent on startup.** Two root causes fixed:
  - `SettingsService.GetSettingsAsync` awaited file I/O that resumed on the UI
    synchronization context, while `CoinGeckoService`/`CryptoCompareService`
    called it synchronously with `.GetAwaiter().GetResult()`. This deadlocked
    the UI thread during view-model construction, so the window was created but
    never painted. Settings access is now fully asynchronous
    (`ApplyApiKeyAsync`), uses `ConfigureAwait(false)`, and is cached.
  - `CoinGeckoService.GetWithRetryAsync` recursed into itself instead of
    performing the HTTP request, causing unbounded recursion/stack growth.

### Added
- API key management UI
- Manual and automatic refresh
- Multiple chart types (sparkline, area, candlestick, donut, bar, gauge, multi-line)
- Portfolio and alert creation forms
- Real global market data and fear & greed index
- Unit test project

### Changed
- Improved dashboard table sorting
- Improved Linux compatibility
