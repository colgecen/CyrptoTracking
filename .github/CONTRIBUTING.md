# Contributing to CryptoTrack

Thanks for your interest in contributing! Here's how you can help.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/CryptoTrack.git`
3. Create a feature branch: `git checkout -b feature/my-feature`
4. Make your changes
5. Run the build: `dotnet build`
6. Commit and push, then open a Pull Request

## Development Setup

- Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Open the solution in your preferred editor (Rider, VS Code, Visual Studio)
- Run with `dotnet run --project AvaloniaAppCrypto`

## Code Style

- Follow existing patterns in the codebase
- Use MVVM with CommunityToolkit.Mvvm
- Keep XAML bindings clean — avoid compiled bindings unless necessary
- Use `DynamicResource` for theme-aware colors
- Place new services behind interfaces in `Interfaces/`

## Pull Request Checklist

- [ ] Build succeeds with no warnings
- [ ] Code follows existing conventions
- [ ] No unnecessary dependencies added
- [ ] Tested locally with `dotnet run`

## Reporting Issues

Use the issue templates — include steps to reproduce, expected behavior, and screenshots if applicable.

## Questions?

Open a discussion or reach out via the project's GitHub Issues page.
