using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

        private bool _hover;

        static GaugeChart()
        {
            ValueProperty.Changed.AddClassHandler<GaugeChart>((s, _) => s.InvalidateVisual());
            AffectsRender<GaugeChart>(ValueProperty);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (!_hover) { _hover = true; InvalidateVisual(); }
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            if (_hover) { _hover = false; InvalidateVisual(); }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var w = Bounds.Width; var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var cx = w / 2;
            var cy = h * 0.85;
            var r = Math.Min(w, h * 1.6) * 0.38;
            var penBg = new Pen(new SolidColorBrush(Color.Parse("#1F2532")), 14);
            var clamped = Math.Clamp(Value, 0, 100);

            // color by value (fear/greed): low=red, mid=yellow, high=green
            Color valColor = clamped < 35 ? Color.Parse("#F6465D")
                           : clamped < 65 ? Color.Parse("#F7D354")
                           : Color.Parse("#0ECB81");
            var penVal = new Pen(new SolidColorBrush(valColor), 14);

            DrawArc(context, cx, cy, r, 180, 180, penBg);
            var sweep = clamped / 100.0 * 180.0;
            DrawArc(context, cx, cy, r, 180, sweep, penVal);

            // value text
            var text = new FormattedText($"{clamped:0}", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface("Inter", FontStyle.Normal, FontWeight.Bold), 28, Brushes.White);
            context.DrawText(text, new Point(cx - text.Width / 2, cy - r * 0.55));

            if (_hover)
            {
                var label = $"Endeks: {clamped:0} / 100";
                DrawTooltip(context, label, new Point(cx, cy - r - 8), w, h, valColor);
            }
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

        private static void DrawTooltip(DrawingContext context, string text, Point anchor, double chartW, double chartH, Color accent)
        {
            var ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface("Inter", FontStyle.Normal, FontWeight.Medium), 11, Brushes.White);
            var padX = 8.0; var padY = 5.0;
            var boxW = ft.Width + padX * 2;
            var boxH = ft.Height + padY * 2;
            var x = anchor.X - boxW / 2;
            var y = anchor.Y - boxH;
            if (x < 4) x = 4;
            if (x + boxW > chartW - 4) x = chartW - boxW - 4;
            if (y < 4) y = 4;
            var rect = new Rect(x, y, boxW, boxH);
            context.DrawRectangle(new SolidColorBrush(Color.FromArgb(235, 18, 22, 31)), new Pen(new SolidColorBrush(accent), 1), rect, 6, 6);
            context.DrawText(ft, new Point(x + padX, y + padY));
        }

        private static Point Polar(double cx, double cy, double r, double deg)
        {
            var rad = deg * Math.PI / 180.0;
            return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
        }
    }
}
