using System.Collections.Generic;

namespace AvaloniaAppCrypto.Models
{
    public class ChartSeries
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#3B82F6";
        public List<double> Values { get; set; } = new();
    }
}