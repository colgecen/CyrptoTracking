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
    public class BarChart : Control
    {
        public static readonly StyledProperty<IList<ChartSegment>> SegmentsProperty =
            AvaloniaProperty.Register<BarChart, IList<ChartSegment>>(nameof(Segments), new List<ChartSegment>());

        public IList<ChartSegment> Segments
        {
            get => GetValue(SegmentsProperty);
            set => SetValue(SegmentsProperty, value);
        }

        private int _hoverIndex = -1;

        static BarChart()
        {
            SegmentsProperty.Changed.AddClassHandler<BarChart>((s, e) => s.OnSegmentsChanged(e));
            AffectsRender<BarChart>(SegmentsProperty);
        }

        private void OnSegmentsChanged(AvaloniaPropertyChangedEventArgs e)
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
            var segments = Segments;
            if (segments == null || segments.Count == 0) return;
            var pos = e.GetPosition(this);
            var rowH = Bounds.Height / segments.Count;
            var idx = Math.Clamp((int)(pos.Y / rowH), 0, segments.Count - 1);
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

            var max = segments.Max(s => Math.Abs(s.Value));
            if (max == 0) max = 1;

            int count = segments.Count;
            double rowH = h / count;
            double barMaxW = w * 0.55;

            for (int i = 0; i < count; i++)
            {
                var seg = segments[i];
                var y = i * rowH + rowH * 0.2;
                var barH = rowH * 0.6;
                var barW = Math.Abs(seg.Value) / max * barMaxW;
                var color = Color.Parse(seg.Color);
                var brush = new SolidColorBrush(color);

                if (i == _hoverIndex)
                {
                    context.DrawRectangle(new SolidColorBrush(Color.FromArgb(25, 255, 255, 255)), null,
                        new Rect(0, i * rowH, w, rowH), 4, 4);
                }

                context.DrawRectangle(brush, null, new Rect(0, y, barW, barH), 4, 4);

                var label = new FormattedText(seg.Label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("Inter", FontStyle.Normal, FontWeight.Medium), 11, Brushes.White);
                context.DrawText(label, new Point(barW + 8, y + barH / 2 - label.Height / 2));

                var valText = $"{seg.Value:+0.00;-0.00;0.00}%";
                var val = new FormattedText(valText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("Inter", FontStyle.Normal, FontWeight.SemiBold), 11, brush);
                context.DrawText(val, new Point(w - val.Width - 4, y + barH / 2 - val.Height / 2));
            }
        }
    }
}
