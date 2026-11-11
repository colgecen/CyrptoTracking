using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaAppCrypto.Converters
{
    /// <summary>
    /// Favorite star accent color. Kept deliberately single-glyph so the
    /// icon size is identical in both states.
    /// </summary>
    public class FavoriteStarColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush Active = new SolidColorBrush(Color.Parse("#F7D354"));
        private static readonly SolidColorBrush Inactive = new SolidColorBrush(Color.Parse("#4B5563"));

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b && b ? Active : Inactive;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
