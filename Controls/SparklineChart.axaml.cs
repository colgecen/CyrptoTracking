using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace AvaloniaAppCrypto.Controls
{
    public class SparklineChart : Control
    {
        public static readonly StyledProperty<IList<double>> ValuesProperty =
            AvaloniaProperty.Register<SparklineChart, IList<double>>(nameof(Values), new List<double>());

        public IList<double> Values
        {
            get => GetValue(ValuesProperty);
            set => SetValue(ValuesProperty, value);
        }

        private List<double> _renderData = new();
        private Point[] _points = Array.Empty<Point>();
        private int _hoverIndex = -1;

        static SparklineChart()
        {
            ValuesProperty.Changed.AddClassHandler<SparklineChart>((s, _) => s.OnDataChanged());
            AffectsRender<SparklineChart>(ValuesProperty);
        }

        private void OnDataChanged()
        {
            _hoverIndex = -1;
            InvalidateVisual();
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_points.Length < 2) return;
            var pos = e.GetPosition(this);
            var idx = (int)Math.Round(pos.X / Bounds.Width * (_points.Length - 1));
            idx = Math.Clamp(idx, 0, _points.Length - 1);
            if (idx != _hoverIndex)
            {
                _hoverIndex = idx;
                InvalidateVisual();
            }
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            if (_hoverIndex != -1)
            {
                _hoverIndex = -1;
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var data = Values;
            if (data == null || data.Count < 2)
            {
                _renderData.Clear();
                _points = Array.Empty<Point>();
                return;
            }

            var list = data.ToList();
            if (list.Count > 200)
            {
                int step = (int)Math.Ceiling(list.Count / 200.0);
                list = list.Where((_, i) => i % step == 0).ToList();
                if (list.Count < 2) return;
            }

            var w = Bounds.Width;
            var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var min = list.Min();
            var max = list.Max();
            var range = max - min;
            if (range == 0) range = 1;

            _renderData = list;
            _points = new Point[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var x = (i / (double)(list.Count - 1)) * w;
                var y = h - ((list[i] - min) / range) * (h - 4) - 2;
                _points[i] = new Point(x, y);
            }

            bool isPositive = list.Last() >= list.First();
            var baseColor = isPositive ? Color.Parse("#0ECB81") : Color.Parse("#F6465D");
            var lineBrush = new SolidColorBrush(baseColor);
            var fillBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Color.FromArgb(80, baseColor.R, baseColor.G, baseColor.B), 0),
                    new GradientStop(Color.FromArgb(10, baseColor.R, baseColor.G, baseColor.B), 1)
                }
            };

            // Fill
            var fillPath = new StreamGeometry();
            using (var ctx = fillPath.Open())
            {
                ctx.BeginFigure(_points[0], true);
                for (int i = 1; i < _points.Length; i++) ctx.LineTo(_points[i]);
                ctx.LineTo(new Point(w, h));
                ctx.LineTo(new Point(0, h));
                ctx.EndFigure(true);
            }
            context.DrawGeometry(fillBrush, null, fillPath);

            // Line
            var linePath = new StreamGeometry();
            using (var ctx = linePath.Open())
            {
                ctx.BeginFigure(_points[0], false);
                for (int i = 1; i < _points.Length; i++) ctx.LineTo(_points[i]);
                ctx.EndFigure(false);
            }
            context.DrawGeometry(null, new Pen(lineBrush, 2), linePath);

            // Hover overlay
            if (_hoverIndex >= 0 && _hoverIndex < _points.Length)
            {
                var p = _points[_hoverIndex];
                var val = _renderData[_hoverIndex];
                var guidePen = new Pen(new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)), 1, dashStyle: new DashStyle(new[] { 4.0, 4.0 }, 0));
                context.DrawLine(guidePen, new Point(p.X, 0), new Point(p.X, h));
                context.DrawEllipse(lineBrush, new Pen(Brushes.White, 2), p, 5, 5);

                var text = $"${val:N2}";
                DrawTooltip(context, text, p, w, h, baseColor);
            }
        }

        private static void DrawTooltip(DrawingContext context, string text, Point anchor, double chartW, double chartH, Color accent)
        {
            var ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface("Inter", FontStyle.Normal, FontWeight.SemiBold), 12, Brushes.White);
            var padX = 8.0; var padY = 5.0;
            var boxW = ft.Width + padX * 2;
            var boxH = ft.Height + padY * 2;
            var x = anchor.X + 10;
            var y = anchor.Y - boxH - 10;
            if (x + boxW > chartW - 4) x = anchor.X - boxW - 10;
            if (y < 4) y = anchor.Y + 10;
            var rect = new Rect(x, y, boxW, boxH);
            context.DrawRectangle(new SolidColorBrush(Color.FromArgb(230, 18, 22, 31)), new Pen(new SolidColorBrush(accent), 1), rect, 6, 6);
            context.DrawText(ft, new Point(x + padX, y + padY));
        }
    }
}
