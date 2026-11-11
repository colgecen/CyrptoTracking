using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Controls
{
    public class MultiLineChart : Control
    {
        public static readonly StyledProperty<List<ChartSeries>> SeriesProperty =
            AvaloniaProperty.Register<MultiLineChart, List<ChartSeries>>(nameof(Series), new List<ChartSeries>());

        public List<ChartSeries> Series
        {
            get => GetValue(SeriesProperty);
            set => SetValue(SeriesProperty, value);
        }

        static MultiLineChart()
        {
            SeriesProperty.Changed.AddClassHandler<MultiLineChart>((s, e) => s.InvalidateVisual());
            AffectsRender<MultiLineChart>(SeriesProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var seriesList = Series;
            if (seriesList == null || seriesList.Count == 0) return;

            var allValues = seriesList.SelectMany(s => s.Values).ToList();
            if (allValues.Count < 2) return;

            var w = Bounds.Width;
            var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var min = allValues.Min();
            var max = allValues.Max();
            var range = max - min;
            if (range == 0) range = 1;

            int pointCount = seriesList.Max(s => s.Values.Count);
            if (pointCount < 2) return;

            foreach (var series in seriesList)
            {
                if (series.Values.Count < 2) continue;
                var vals = series.Values;
                var pts = new Point[vals.Count];
                for (int i = 0; i < vals.Count; i++)
                {
                    var x = (i / (double)(vals.Count - 1)) * w;
                    var y = h - ((vals[i] - min) / range) * (h - 8) - 4;
                    pts[i] = new Point(x, y);
                }
                var color = Color.Parse(series.Color);
                var pen = new Pen(new SolidColorBrush(color), 2);
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(pts[0], false);
                    for (int i = 1; i < pts.Length; i++)
                        ctx.LineTo(pts[i]);
                }
                context.DrawGeometry(null, pen, geometry);
            }
        }
    }
}