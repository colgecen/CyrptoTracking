namespace AvaloniaAppCrypto.Interfaces
{
    public interface IThemeService
    {
        void SetTheme(string theme); // "Light", "Dark", "System"
        string CurrentTheme { get; }
    }
}
