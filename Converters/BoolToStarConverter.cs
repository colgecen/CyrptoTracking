using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaAppCrypto.Converters
{
    public class BoolToStarConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? "⭐" : "☆";
            return "☆";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
