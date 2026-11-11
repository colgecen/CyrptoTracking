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
- **No live data / empty charts.** `HttpClient.Timeout` was reassigned on every
  request (throws once the client has been used) and the period buttons passed
  a string `CommandParameter` to `int` relay commands. Timeouts are now
  per-request, API keys travel with the request, and period commands parse
  their argument. Candlestick and comparison charts load at startup.
- Dashboard no longer opens scrolled down; sidebar highlights the active page.

### Changed
- Coins update in place instead of being rebuilt every refresh; auto-refresh no
  longer overlaps, news reloads at most every 5 minutes, refresh interval has a
  15 s floor. Idle CPU dropped from ~11% to ~1.7%.
- Linux builds use Avalonia's CPU renderer by default because XWayland/GLX
  presented stale frames on some Intel/Mesa setups. Set `CRYPTOTRACK_GLX=1` to
  force GLX.
- Improved dashboard table sorting
- Improved Linux compatibility

### Added
- API key management UI
- Manual and automatic refresh
- Multiple chart types (sparkline, area, candlestick, donut, bar, gauge, multi-line)
- Portfolio and alert creation forms
- Real global market data and fear & greed index
- Unit test project
