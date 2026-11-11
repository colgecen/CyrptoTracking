using System;

namespace AvaloniaAppCrypto.Models
{
    public class ChartSegment
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Color { get; set; } = "#3B82F6";
    }
}