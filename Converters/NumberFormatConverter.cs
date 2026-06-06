using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaAppCrypto.Converters
{
    public class NumberFormatConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return FormatNumber(d);
            if (value is long l)
                return FormatNumber((decimal)l);
            if (value is double dbl)
                return FormatNumber((decimal)dbl);
            return value?.ToString() ?? "";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();

        private static string FormatNumber(decimal value)
        {
            if (Math.Abs(value) >= 1_000_000_000_000)
                return $"${value / 1_000_000_000_000m:F2}T";
            if (Math.Abs(value) >= 1_000_000_000)
                return $"${value / 1_000_000_000m:F2}B";
            if (Math.Abs(value) >= 1_000_000)
                return $"${value / 1_000_000m:F2}M";
            if (Math.Abs(value) >= 1_000)
                return $"${value / 1_000m:F2}K";
            if (value >= 1)
                return $"${value:F2}";
            return $"${value:F6}";
        }
    }

    public class PercentageFormatConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double dbl)
                return $"{dbl:F2}%";
            if (value is decimal dec)
                return $"{dec:F2}%";
            return value?.ToString() ?? "0%";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
