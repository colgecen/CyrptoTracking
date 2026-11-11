using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaAppCrypto.Converters
{
    public class ActiveNavForegroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is true)
                return new SolidColorBrush(Color.Parse("#60A5FA"));
            return new SolidColorBrush(Color.Parse("#9CA3AF"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
