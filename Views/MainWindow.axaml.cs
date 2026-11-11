using Avalonia.Controls;
using AvaloniaAppCrypto.ViewModels;

namespace AvaloniaAppCrypto.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0F1117"));
            TransparencyLevelHint = new[] { WindowTransparencyLevel.None };
        }
    }
}
