using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Controls
{
    public class DonutChart : Control
    {
        public static readonly StyledProperty<IList<ChartSegment>> SegmentsProperty =
            AvaloniaProperty.Register<DonutChart, IList<ChartSegment>>(nameof(Segments), new List<ChartSegment>());

        public IList<ChartSegment> Segments
        {
            get => GetValue(SegmentsProperty);
            set => SetValue(SegmentsProperty, value);
        }

        private int _hoverIndex = -1;

        static DonutChart()
        {
            SegmentsProperty.Changed.AddClassHandler<DonutChart>((s, _) => { s._hoverIndex = -1; s.InvalidateVisual(); });
            AffectsRender<DonutChart>(SegmentsProperty);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            var segments = Segments;
            if (segments == null || segments.Count == 0) return;
            var pos = e.GetPosition(this);
            var w = Bounds.Width; var h = Bounds.Height;
            var cx = w / 2; var cy = h / 2;
            var dx = pos.X - cx; var dy = pos.Y - cy;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            var radius = Math.Min(w, h) / 2 - 8;
            var inner = radius * 0.6;
            if (dist < inner || dist > radius) { if (_hoverIndex != -1) { _hoverIndex = -1; InvalidateVisual(); } return; }

            var angle = Math.Atan2(dy, dx) * 180.0 / Math.PI; // -180..180, 0 at right
            angle = (angle + 90 + 360) % 360; // 0 at top, clockwise
            var total = segments.Sum(s => s.Value);
            if (total <= 0) return;
            double acc = 0;
            int idx = -1;
            for (int i = 0; i < segments.Count; i++)
            {
                var sweep = segments[i].Value / total * 360.0;
                if (angle >= acc && angle < acc + sweep) { idx = i; break; }
                acc += sweep;
            }
            if (idx == -1) idx = segments.Count - 1;
            if (idx != _hoverIndex) { _hoverIndex = idx; InvalidateVisual(); }
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            if (_hoverIndex != -1) { _hoverIndex = -1; InvalidateVisual(); }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var segments = Segments;
            if (segments == null || segments.Count == 0) return;

            var w = Bounds.Width; var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var cx = w / 2; var cy = h / 2;
            var radius = Math.Min(w, h) / 2 - 8;
            var inner = radius * 0.6;
            var total = segments.Sum(s => s.Value);
            if (total <= 0) return;

            double startAngle = -90;
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                var sweep = (seg.Value / total) * 360.0;
                var r = (i == _hoverIndex) ? radius + 4 : radius;
                DrawArc(context, cx, cy, r, inner, startAngle, sweep, Color.Parse(seg.Color));
                startAngle += sweep;
            }

            if (_hoverIndex >= 0 && _hoverIndex < segments.Count)
            {
                var seg = segments[_hoverIndex];
                var pct = seg.Value / total * 100.0;
                var text = $"{seg.Label}\n{pct:F1}%";
                DrawCenterTooltip(context, text, cx, cy, Color.Parse(seg.Color));
            }
        }

        private static void DrawArc(DrawingContext context, double cx, double cy, double outerR, double innerR, double startDeg, double sweepDeg, Color color)
        {
            if (sweepDeg <= 0) return;
            if (sweepDeg >= 359.99) sweepDeg = 359.99; // avoid full circle arc issue
            var brush = new SolidColorBrush(color);
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                var startOuter = Polar(cx, cy, outerR, startDeg);
                var endOuter = Polar(cx, cy, outerR, startDeg + sweepDeg);
                var startInner = Polar(cx, cy, innerR, startDeg + sweepDeg);
                var endInner = Polar(cx, cy, innerR, startDeg);

                ctx.BeginFigure(startOuter, true);
                ctx.ArcTo(endOuter, new Size(outerR, outerR), 0, sweepDeg > 180, SweepDirection.Clockwise);
                ctx.LineTo(startInner);
                ctx.ArcTo(endInner, new Size(innerR, innerR), 0, sweepDeg > 180, SweepDirection.CounterClockwise);
                ctx.EndFigure(true);
            }
            context.DrawGeometry(brush, null, geometry);
        }

        private static void DrawCenterTooltip(DrawingContext context, string text, double cx, double cy, Color accent)
        {
            var ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface("Inter", FontStyle.Normal, FontWeight.SemiBold), 13, Brushes.White)
            { TextAlignment = TextAlignment.Center };
            context.DrawText(ft, new Point(cx, cy - ft.Height / 2));
        }

        private static Point Polar(double cx, double cy, double r, double deg)
        {
            var rad = deg * Math.PI / 180.0;
            return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
        }
    }
}
