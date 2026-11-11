using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAppCrypto.Models
{
    /// <summary>
    /// A single entry in the sidebar navigation.
    /// </summary>
    public partial class NavigationItem : ObservableObject
    {
        public string Page { get; init; } = string.Empty;
        public string Icon { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        // SVG path data for a consistent star-like / iconographic silhouette.
        public string IconData { get; init; } = string.Empty;

        [ObservableProperty]
        private bool _isActive;
    }
}
