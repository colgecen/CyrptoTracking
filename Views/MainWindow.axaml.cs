using System;
using Avalonia.Controls;
using AvaloniaAppCrypto.ViewModels;

namespace AvaloniaAppCrypto.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0F1117"));
            TransparencyLevelHint = new[] { WindowTransparencyLevel.None };
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("/tmp/opencode/mainwindow_error.txt", ex.ToString());
                throw;
            }
        }
    }
}
