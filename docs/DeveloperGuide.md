# CryptoTrack Geliştirici Rehberi

## Mimari
- MVVM + CommunityToolkit.Mvvm
- DI: Microsoft.Extensions.DependencyInjection
- Servisler: Interfaces/ ve Services/
- ViewModels: ViewModels/MainWindowViewModel.cs

## Build
```bash
dotnet build
dotnet test Tests/AvaloniaAppCrypto.Tests.csproj
```

## Commit Kuralı
Her görev için `feat(scope): açıklama (Task ID)` formatında commit ve push.