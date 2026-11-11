using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaAppCrypto.Converters
{
    /// <summary>
    /// Active sidebar item accent bar brush.
    /// </summary>
    public class ActiveNavAccentConverter : IValueConverter
    {
        private static readonly IBrush Active = new SolidColorBrush(Color.Parse("#00D1FF"));
        private static readonly IBrush Inactive = Brushes.Transparent;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b && b ? Active : Inactive;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
