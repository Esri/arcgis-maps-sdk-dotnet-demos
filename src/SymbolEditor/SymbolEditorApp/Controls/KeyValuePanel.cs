using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SymbolEditorApp.Controls
{
    public class KeyValuePanel : Panel
    {
        protected override Size MeasureOverride(System.Windows.Size availableSize)
        {
            double colWidth1 = double.NaN;
            double colWidth2 = double.NaN;
            if (KeyColumnWidth.IsAbsolute)
                colWidth1 = KeyColumnWidth.Value;
            if (ValueColumnWidth.IsAbsolute)
                colWidth2 = KeyColumnWidth.Value;
            if(double.IsNaN(colWidth1) || double.IsNaN(colWidth2))
            {
                if (!double.IsNaN(colWidth2))
                    colWidth1 = availableSize.Width - colWidth2;
                else if (!double.IsNaN(colWidth1))
                {
                    colWidth2 = availableSize.Width - colWidth1;
                }
                else
                {
                    var star = availableSize.Width / (KeyColumnWidth.Value + ValueColumnWidth.Value);
                    colWidth1 = star * KeyColumnWidth.Value;
                    colWidth2 = star * ValueColumnWidth.Value;
                }
            }
            double y = 0;
            for (int i = 0; i < Children.Count; i+=2)
            {
                UIElement key = Children[i] as UIElement;
                var span = Grid.GetColumnSpan(key);
                UIElement value = null;
                if (Children.Count > i && span < 2)
                    value = Children[i + 1];
                if (span > 1)
                {
                    key.Measure(new Size(availableSize.Width, availableSize.Height - y));
                    i--;
                }
                else
                {
                    key.Measure(new Size(colWidth1, availableSize.Height - y));
                    value?.Measure(new Size(colWidth2, availableSize.Height - y));
                }
                y += Math.Max(key.DesiredSize.Height, value?.DesiredSize.Height ?? 0);
            }
            return new Size(colWidth1 + colWidth2, y);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            double colWidth1 = double.NaN;
            double colWidth2 = double.NaN;
            if (KeyColumnWidth.IsAbsolute)
                colWidth1 = KeyColumnWidth.Value;
            if (ValueColumnWidth.IsAbsolute)
                colWidth2 = KeyColumnWidth.Value;
            if (double.IsNaN(colWidth1) || double.IsNaN(colWidth2))
            {
                if (!double.IsNaN(colWidth2))
                    colWidth1 = finalSize.Width - colWidth2;
                else if (!double.IsNaN(colWidth1))
                {
                    colWidth2 = finalSize.Width - colWidth1;
                }
                else
                {
                    var star = finalSize.Width / (KeyColumnWidth.Value + ValueColumnWidth.Value);
                    colWidth1 = star * KeyColumnWidth.Value;
                    colWidth2 = star * ValueColumnWidth.Value;
                }
            }
            double y = 0;
            for (int i = 0; i < Children.Count; i += 2)
            {
                UIElement key = Children[i] as UIElement;
                var span = Grid.GetColumnSpan(key);
                UIElement value = null;
                if (Children.Count > i && span < 2)
                    value = Children[i + 1];
                double rowHeight = 0;
                if (span > 1)
                {
                    key.Arrange(new Rect(0, y, finalSize.Width, key.DesiredSize.Height));
                    rowHeight = key.DesiredSize.Height;
                    i--;
                }
                else
                {
                    rowHeight = Math.Max(key.DesiredSize.Height, value?.DesiredSize.Height ?? 0);
                    key.Arrange(new Rect(0, y, colWidth1, rowHeight));
                    value?.Arrange(new Rect(colWidth1, y, colWidth2, rowHeight));
                }
                y += rowHeight;
            }
            return new Size(colWidth1 + colWidth2, y);
        }

        public GridLength KeyColumnWidth
        {
            get { return (GridLength)GetValue(KeyColumnWidthProperty); }
            set { SetValue(KeyColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty KeyColumnWidthProperty =
            DependencyProperty.Register("KeyColumnWidth", typeof(GridLength), typeof(KeyValuePanel), new PropertyMetadata(new GridLength(1, GridUnitType.Star), InvalidateMeasure));

        public GridLength ValueColumnWidth
        {
            get { return (GridLength)GetValue(ValueColumnWidthProperty); }
            set { SetValue(ValueColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty ValueColumnWidthProperty =
            DependencyProperty.Register("ValueColumnWidth", typeof(GridLength), typeof(KeyValuePanel), new PropertyMetadata(new GridLength(1, GridUnitType.Star), InvalidateMeasure));

        private static void InvalidateMeasure(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((KeyValuePanel)d).InvalidateMeasure();
    }
}
