using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Controls
{
    public class CandlestickChart : Control
    {
        public static readonly StyledProperty<IList<OhlcPoint>> CandlesProperty =
            AvaloniaProperty.Register<CandlestickChart, IList<OhlcPoint>>(nameof(Candles), new List<OhlcPoint>());

        public IList<OhlcPoint> Candles
        {
            get => GetValue(CandlesProperty);
            set => SetValue(CandlesProperty, value);
        }

        static CandlestickChart()
        {
            CandlesProperty.Changed.AddClassHandler<CandlestickChart>((s, e) => s.InvalidateVisual());
            AffectsRender<CandlestickChart>(CandlesProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var candles = Candles;
            if (candles == null || candles.Count < 2) return;

            var w = Bounds.Width;
            var h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var min = candles.Min(c => c.Low);
            var max = candles.Max(c => c.High);
            var range = max - min;
            if (range == 0) range = 1;

            var candleWidth = Math.Max(2, w / candles.Count * 0.6);
            var step = w / candles.Count;

            for (int i = 0; i < candles.Count; i++)
            {
                var c = candles[i];
                var x = i * step + step / 2;
                var yHigh = h - ((c.High - min) / range) * (h - 8) - 4;
                var yLow = h - ((c.Low - min) / range) * (h - 8) - 4;
                var yOpen = h - ((c.Open - min) / range) * (h - 8) - 4;
                var yClose = h - ((c.Close - min) / range) * (h - 8) - 4;

                var isUp = c.Close >= c.Open;
                var color = isUp ? Color.Parse("#10B981") : Color.Parse("#EF4444");
                var brush = new SolidColorBrush(color);
                var pen = new Pen(brush, 1);

                // wick
                context.DrawLine(pen, new Point(x, yHigh), new Point(x, yLow));

                // body
                var top = Math.Min(yOpen, yClose);
                var bottom = Math.Max(yOpen, yClose);
                var rect = new Rect(x - candleWidth / 2, top, candleWidth, Math.Max(1, bottom - top));
                context.DrawRectangle(brush, null, rect);
            }
        }
    }
}