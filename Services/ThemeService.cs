using Avalonia;
using Avalonia.Styling;
using AvaloniaAppCrypto.Interfaces;

namespace AvaloniaAppCrypto.Services
{
    public class ThemeService : IThemeService
    {
        public string CurrentTheme { get; private set; } = "Dark";

        public void SetTheme(string theme)
        {
            CurrentTheme = theme;
            if (Application.Current == null) return;

            Application.Current.RequestedThemeVariant = theme switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
        }
    }
}
