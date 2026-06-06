using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaAppCrypto.Converters
{
    public class StringEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s && parameter is string p)
                return s == p;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
