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
        var tracePath = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "CryptoTrack", "trace.log");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(tracePath)!);
        System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(tracePath));
        System.Diagnostics.Trace.AutoFlush = true;

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
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if DEBUG
                .WithDeveloperTools()
#endif
                .WithInterFont()
                .LogToTrace();
    }
}
