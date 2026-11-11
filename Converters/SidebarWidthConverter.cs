using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaAppCrypto.Converters
{
    public class SidebarWidthConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b && b ? new Avalonia.Controls.GridLength(0) : new Avalonia.Controls.GridLength(220);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
