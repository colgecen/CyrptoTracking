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
    public class CandlestickChart : Control
    {
        private static readonly SolidColorBrush s_upBrush = new SolidColorBrush(Color.Parse("#0ECB81"));
        private static readonly SolidColorBrush s_downBrush = new SolidColorBrush(Color.Parse("#F6465D"));
        public static readonly StyledProperty<IList<OhlcPoint>> CandlesProperty =
            AvaloniaProperty.Register<CandlestickChart, IList<OhlcPoint>>(nameof(Candles), new List<OhlcPoint>());

        public IList<OhlcPoint> Candles
        {
            get => GetValue(CandlesProperty);
            set => SetValue(CandlesProperty, value);
        }

        private int _hoverIndex = -1;

        static CandlestickChart()
        {
            CandlesProperty.Changed.AddClassHandler<CandlestickChart>((s, _) => { s._hoverIndex = -1; s.InvalidateVisual(); });
            AffectsRender<CandlestickChart>(CandlesProperty);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            var candles = Candles;
            if (candles == null || candles.Count == 0) return;
            var pos = e.GetPosition(this);
            var step = Bounds.Width / candles.Count;
            var idx = Math.Clamp((int)(pos.X / step), 0, candles.Count - 1);
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
            var candles = Candles;
            if (candles == null || candles.Count < 2) return;

            var w = Bounds.Width; var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var min = candles.Min(c => c.Low);
            var max = candles.Max(c => c.High);
            var range = max - min;
            if (range == 0) range = 1;

            var candleWidth = Math.Max(2, w / candles.Count * 0.6);
            var step = w / candles.Count;

            double Y(double val) => h - ((val - min) / range) * (h - 8) - 4;

            for (int i = 0; i < candles.Count; i++)
            {
                var c = candles[i];
                var x = i * step + step / 2;
                var yHigh = Y(c.High); var yLow = Y(c.Low);
                var yOpen = Y(c.Open); var yClose = Y(c.Close);

                var isUp = c.Close >= c.Open;
                var brush = isUp ? s_upBrush : s_downBrush;
                var pen = new Pen(brush, 1);

                // highlight hover column
                if (i == _hoverIndex)
                {
                    context.DrawRectangle(new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)), null,
                        new Rect(i * step, 0, step, h));
                }

                context.DrawLine(pen, new Point(x, yHigh), new Point(x, yLow));
                var top = Math.Min(yOpen, yClose);
                var bottom = Math.Max(yOpen, yClose);
                var rect = new Rect(x - candleWidth / 2, top, candleWidth, Math.Max(1, bottom - top));
                context.DrawRectangle(brush, null, rect);
            }

            if (_hoverIndex >= 0 && _hoverIndex < candles.Count)
            {
                var c = candles[_hoverIndex];
                var x = _hoverIndex * step + step / 2;
                var guidePen = new Pen(new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)), 1, dashStyle: new DashStyle(new[] { 4.0, 4.0 }, 0));
                context.DrawLine(guidePen, new Point(x, 0), new Point(x, h));

                var isUp = c.Close >= c.Open;
                var accent = isUp ? Color.Parse("#0ECB81") : Color.Parse("#F6465D");
                var text = $"{c.Time:dd MMM HH:mm}\nAçılış: ${c.Open:N2}\nYüksek: ${c.High:N2}\nDüşük:  ${c.Low:N2}\nKapanış: ${c.Close:N2}";
                DrawTooltip(context, text, new Point(x, Y(c.High)), w, h, accent);
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
            var y = anchor.Y - boxH / 2;
            if (x + boxW > chartW - 4) x = anchor.X - boxW - 12;
            if (y < 4) y = 4;
            if (y + boxH > chartH - 4) y = chartH - boxH - 4;
            var rect = new Rect(x, y, boxW, boxH);
            context.DrawRectangle(new SolidColorBrush(Color.FromArgb(235, 18, 22, 31)), new Pen(new SolidColorBrush(accent), 1), rect, 6, 6);
            context.DrawText(ft, new Point(x + padX, y + padY));
        }
    }
}
