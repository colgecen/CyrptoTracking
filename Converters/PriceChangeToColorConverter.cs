using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaAppCrypto.Converters
{
    public class PriceChangeToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                if (d > 0) return new SolidColorBrush(Color.Parse("#10B981"));
                if (d < 0) return new SolidColorBrush(Color.Parse("#EF4444"));
                return new SolidColorBrush(Color.Parse("#9CA3AF"));
            }
            return new SolidColorBrush(Color.Parse("#9CA3AF"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class PriceChangeToBackgroundColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                if (d > 0) return new SolidColorBrush(Color.Parse("#10B981"));
                if (d < 0) return new SolidColorBrush(Color.Parse("#EF4444"));
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
