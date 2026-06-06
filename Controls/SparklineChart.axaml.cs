using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaAppCrypto.Controls
{
    public class SparklineChart : Control
    {
        public static readonly StyledProperty<List<double>> ValuesProperty =
            AvaloniaProperty.Register<SparklineChart, List<double>>(nameof(Values), new List<double>());

        public List<double> Values
        {
            get => GetValue(ValuesProperty);
            set => SetValue(ValuesProperty, value);
        }

        static SparklineChart()
        {
            ValuesProperty.Changed.AddClassHandler<SparklineChart>((s, e) => s.InvalidateVisual());
            AffectsRender<SparklineChart>(ValuesProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var data = Values;
            if (data == null || data.Count < 2) return;

            var w = Bounds.Width;
            var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var min = data.Min();
            var max = data.Max();
            var range = max - min;
            if (range == 0) range = 1;

            var pts = new Point[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                var x = (i / (double)(data.Count - 1)) * w;
                var y = h - ((data[i] - min) / range) * (h - 4) - 2;
                pts[i] = new Point(x, y);
            }

            bool isPositive = data.Last() >= data.First();
            var lineColor = new SolidColorBrush(isPositive ? Color.Parse("#10B981") : Color.Parse("#EF4444"));
            var fillColor = new SolidColorBrush(isPositive ? Color.Parse("#10B981") : Color.Parse("#EF4444"), 0.15);

            // Draw fill
            var fillPath = new StreamGeometry();
            using (var ctx = fillPath.Open())
            {
                ctx.BeginFigure(pts[0], true);
                for (int i = 1; i < pts.Length; i++)
                    ctx.LineTo(pts[i]);
                ctx.LineTo(new Point(w, h));
                ctx.LineTo(new Point(0, h));
                ctx.EndFigure(true);
            }
            context.DrawGeometry(fillColor, null, fillPath);

            // Draw line
            var linePath = new StreamGeometry();
            using (var ctx = linePath.Open())
            {
                ctx.BeginFigure(pts[0], false);
                for (int i = 1; i < pts.Length; i++)
                    ctx.LineTo(pts[i]);
                ctx.EndFigure(false);
            }
            context.DrawGeometry(null, new Pen(lineColor, 2), linePath);
        }
    }
}
