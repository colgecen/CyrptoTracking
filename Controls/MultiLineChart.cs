using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Controls
{
    public class MultiLineChart : Control
    {
        private static readonly Pen s_guidePen = new Pen(new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)), 1, dashStyle: new DashStyle(new[] { 4.0, 4.0 }, 0));
        public static readonly StyledProperty<IList<ChartSeries>> SeriesProperty =
            AvaloniaProperty.Register<MultiLineChart, IList<ChartSeries>>(nameof(Series), new List<ChartSeries>());

        public IList<ChartSeries> Series
        {
            get => GetValue(SeriesProperty);
            set => SetValue(SeriesProperty, value);
        }

        private int _hoverIndex = -1;
        private int _pointCount = 0;

        static MultiLineChart()
        {
            SeriesProperty.Changed.AddClassHandler<MultiLineChart>((s, e) => s.OnSeriesChanged(e));
            AffectsRender<MultiLineChart>(SeriesProperty);
        }

        private void OnSeriesChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged oldIncc)
                oldIncc.CollectionChanged -= OnCollectionChanged;
            if (e.NewValue is INotifyCollectionChanged newIncc)
                newIncc.CollectionChanged += OnCollectionChanged;
            _hoverIndex = -1;
            InvalidateVisual();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _hoverIndex = -1;
            InvalidateVisual();
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_pointCount < 2) return;
            var pos = e.GetPosition(this);
            var idx = (int)Math.Round(pos.X / Bounds.Width * (_pointCount - 1));
            idx = Math.Clamp(idx, 0, _pointCount - 1);
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
            var seriesList = Series;
            if (seriesList == null || seriesList.Count == 0) { _pointCount = 0; return; }

            var allValues = seriesList.SelectMany(s => s.Values).ToList();
            if (allValues.Count < 2) { _pointCount = 0; return; }

            var w = Bounds.Width; var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var min = allValues.Min();
            var max = allValues.Max();
            var range = max - min;
            if (range == 0) range = 1;

            _pointCount = seriesList.Max(s => s.Values.Count);
            if (_pointCount < 2) return;

            double Y(double v) => h - ((v - min) / range) * (h - 8) - 4;

            // draw lines
            foreach (var series in seriesList)
            {
                if (series.Values.Count < 2) continue;
                var vals = series.Values;
                var pts = new Point[vals.Count];
                for (int i = 0; i < vals.Count; i++)
                {
                    var x = (i / (double)(vals.Count - 1)) * w;
                    pts[i] = new Point(x, Y(vals[i]));
                }
                var color = Color.Parse(series.Color);
                var pen = new Pen(new SolidColorBrush(color), 2);
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(pts[0], false);
                    for (int i = 1; i < pts.Length; i++) ctx.LineTo(pts[i]);
                }
                context.DrawGeometry(null, pen, geometry);
            }

            // hover
            if (_hoverIndex >= 0 && _hoverIndex < _pointCount)
            {
                var x = (_hoverIndex / (double)(_pointCount - 1)) * w;
                context.DrawLine(s_guidePen, new Point(x, 0), new Point(x, h));

                var lines = new List<string>();
                foreach (var series in seriesList)
                {
                    if (series.Values.Count == 0) continue;
                    var idx = Math.Min(_hoverIndex, series.Values.Count - 1);
                    var val = series.Values[idx];
                    lines.Add($"{series.Name}: ${val:N0}");
                    var color = Color.Parse(series.Color);
                    context.DrawEllipse(new SolidColorBrush(color), new Pen(Brushes.White, 2),
                        new Point(x, Y(val)), 4, 4);
                }

                if (lines.Count > 0)
                {
                    var text = string.Join("\n", lines);
                    var anchor = new Point(x, 10);
                    DrawTooltip(context, text, anchor, w, h, Color.Parse("#00D1FF"));
                }
            }
        }

        private static void DrawTooltip(DrawingContext context, string text, Point anchor, double chartW, double chartH, Color accent)
        {
            var ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface("Inter", FontStyle.Normal, FontWeight.Medium), 11, Brushes.White);
            var padX = 10.0; var padY = 8.0;
            var boxW = ft.Width + padX * 2;
            var boxH = ft.Height + padY * 2;
            var x = anchor.X + 12;
            var y = anchor.Y;
            if (x + boxW > chartW - 4) x = anchor.X - boxW - 12;
            if (y < 4) y = 4;
            if (y + boxH > chartH - 4) y = chartH - boxH - 4;
            var rect = new Rect(x, y, boxW, boxH);
            context.DrawRectangle(new SolidColorBrush(Color.FromArgb(235, 18, 22, 31)), new Pen(new SolidColorBrush(accent), 1), rect, 6, 6);
            context.DrawText(ft, new Point(x + padX, y + padY));
        }
    }
}
