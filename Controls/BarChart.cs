using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Controls
{
    public class BarChart : Control
    {
        public static readonly StyledProperty<IList<ChartSegment>> SegmentsProperty =
            AvaloniaProperty.Register<BarChart, IList<ChartSegment>>(nameof(Segments), new List<ChartSegment>());

        public IList<ChartSegment> Segments
        {
            get => GetValue(SegmentsProperty);
            set => SetValue(SegmentsProperty, value);
        }

        static BarChart()
        {
            SegmentsProperty.Changed.AddClassHandler<BarChart>((s, e) => s.InvalidateVisual());
            AffectsRender<BarChart>(SegmentsProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var segments = Segments;
            if (segments == null || segments.Count == 0) return;

            var w = Bounds.Width;
            var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var max = segments.Max(s => Math.Abs(s.Value));
            if (max == 0) max = 1;

            int count = segments.Count;
            double rowH = h / count;
            double barMaxW = w * 0.6;

            for (int i = 0; i < count; i++)
            {
                var seg = segments[i];
                var y = i * rowH + rowH * 0.2;
                var barH = rowH * 0.6;
                var barW = Math.Abs(seg.Value) / max * barMaxW;
                var color = Color.Parse(seg.Color);
                var rect = new Rect(0, y, barW, barH);
                context.DrawRectangle(new SolidColorBrush(color), null, rect);

                var text = new FormattedText(
                    $"{seg.Label}  {seg.Value:F2}%",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter"),
                    11,
                    Brushes.White);
                context.DrawText(text, new Point(barW + 6, y + 2));
            }
        }
    }
}