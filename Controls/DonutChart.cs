using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
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

        static DonutChart()
        {
            SegmentsProperty.Changed.AddClassHandler<DonutChart>((s, e) => s.InvalidateVisual());
            AffectsRender<DonutChart>(SegmentsProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var segments = Segments;
            if (segments == null || segments.Count == 0) return;

            var w = Bounds.Width;
            var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var cx = w / 2;
            var cy = h / 2;
            var radius = Math.Min(w, h) / 2 - 8;
            var inner = radius * 0.6;
            var total = segments.Sum(s => s.Value);
            if (total <= 0) return;

            double startAngle = -90;
            foreach (var seg in segments)
            {
                var sweep = (seg.Value / total) * 360.0;
                DrawArc(context, cx, cy, radius, inner, startAngle, sweep, Color.Parse(seg.Color));
                startAngle += sweep;
            }
        }

        private static void DrawArc(DrawingContext context, double cx, double cy, double outerR, double innerR, double startDeg, double sweepDeg, Color color)
        {
            if (sweepDeg <= 0) return;
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

        private static Point Polar(double cx, double cy, double r, double deg)
        {
            var rad = deg * Math.PI / 180.0;
            return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
        }
    }
}