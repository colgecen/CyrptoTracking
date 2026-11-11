using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaAppCrypto.Converters
{
    public class SidebarWidthConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b && b ? 0 : 220;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
