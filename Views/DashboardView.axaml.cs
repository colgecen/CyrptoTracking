using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvaloniaAppCrypto.ViewModels;

namespace AvaloniaAppCrypto.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            PageScrollHelper.StartAtTop(this);
        }
    }

    /// <summary>
    /// When a view is shown, the first focusable control can pull its
    /// ScrollViewer down so the page opens mid-way. Reset to the top on load.
    /// </summary>
    internal static class PageScrollHelper
    {
        public static void StartAtTop(Control view)
        {
            view.Loaded += (_, _) =>
            {
                var scroll = view.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                if (scroll != null)
                    scroll.Offset = default;
            };
        }
    }
}
