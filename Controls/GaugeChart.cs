using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaAppCrypto.Controls
{
    public class GaugeChart : Control
    {
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<GaugeChart, double>(nameof(Value), 50);

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        static GaugeChart()
        {
            ValueProperty.Changed.AddClassHandler<GaugeChart>((s, e) => s.InvalidateVisual());
            AffectsRender<GaugeChart>(ValueProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var w = Bounds.Width;
            var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var cx = w / 2;
            var cy = h * 0.8;
            var r = Math.Min(w, h) * 0.4;
            var penBg = new Pen(new SolidColorBrush(Color.Parse("#2D3041")), 14);
            var penVal = new Pen(new SolidColorBrush(Color.Parse("#3B82F6")), 14);

            // background arc 180 deg
            DrawArc(context, cx, cy, r, 180, 180, penBg);
            var clamped = Math.Clamp(Value, 0, 100);
            var sweep = clamped / 100.0 * 180.0;
            DrawArc(context, cx, cy, r, 180, sweep, penVal);

            // text
            var text = new FormattedText(
                $"{clamped:0}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter"),
                24,
                Brushes.White);
            context.DrawText(text, new Point(cx - text.Width / 2, cy - r * 0.5));
        }

        private static void DrawArc(DrawingContext context, double cx, double cy, double r, double startDeg, double sweepDeg, Pen pen)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                var start = Polar(cx, cy, r, startDeg);
                var end = Polar(cx, cy, r, startDeg + sweepDeg);
                ctx.BeginFigure(start, false);
                ctx.ArcTo(end, new Size(r, r), 0, sweepDeg > 180, SweepDirection.Clockwise);
            }
            context.DrawGeometry(null, pen, geometry);
        }

        private static Point Polar(double cx, double cy, double r, double deg)
        {
            var rad = deg * Math.PI / 180.0;
            return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
        }
    }
}