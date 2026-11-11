using Avalonia;
using Avalonia.Controls;

namespace AvaloniaAppCrypto.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0F1117"));
            TransparencyLevelHint = new[] { WindowTransparencyLevel.None };
            InitializeComponent();
        }
    }
}
