using System;
using Avalonia;

namespace AvaloniaAppCrypto
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                try
                {
                    var path = System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                        "CryptoTrack", "crash.log");
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                    System.IO.File.AppendAllText(path, $"{DateTime.Now}: {e.ExceptionObject}\n");
                }
                catch { }
            };

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if DEBUG
                .WithDeveloperTools()
#endif
                .WithInterFont()
                .LogToTrace();

            // On XWayland the GLX swap chain can present stale frames on some
            // Intel/Mesa setups (the window shows outdated or blank content).
            // Avalonia's built-in CPU renderer is fast enough for this UI and
            // is consistent, so use it by default on Linux.
            // Set CRYPTOTRACK_GLX=1 to force hardware GLX rendering.
            if (OperatingSystem.IsLinux()
                && Environment.GetEnvironmentVariable("CRYPTOTRACK_GLX") != "1")
            {
                builder = builder.With(new Avalonia.X11PlatformOptions
                {
                    RenderingMode = new[] { Avalonia.X11RenderingMode.Software }
                });
            }

            return builder;
        }
    }
}
