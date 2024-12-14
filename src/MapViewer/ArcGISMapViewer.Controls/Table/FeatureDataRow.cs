using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace ArcGISMapViewer.Controls
{
    public sealed partial class FeatureDataRow : StackPanel
    {
        public FeatureDataRow()
        {
            this.Height = 24;
            Orientation = Orientation.Horizontal;
            this.BorderThickness = new Thickness(1);
            this.Padding = new Thickness(-1);

            this.PointerEntered += FeatureDataRow_PointerEntered;
            this.PointerExited += FeatureDataRow_PointerExited;
        }

        private void FeatureDataRow_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this.BorderBrush = null;
        }

        private void FeatureDataRow_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black);
        }
        

        private Feature? _Feature;

        internal Feature? Feature
        {
            get => _Feature;
            set
            {   
                if (_Feature != value)
                {
                    _Feature = value;
                    OnFeaturePropertyChanged(value);
                }
            }
        }

        private static readonly DependencyProperty FeatureProperty =
            DependencyProperty.Register(nameof(Feature), typeof(Feature), typeof(FeatureDataRow), new PropertyMetadata(null));

        private void OnFeaturePropertyChanged(Feature? newValue)
        {
            if (cells is null)
                return;
            foreach (var textBlock in cells.OfType<TextCell>())
            {
                var field = textBlock.Tag as Field;
                if (field is not null)
                    textBlock.Text = GetDisplayValue(field);
            }
        }

        private Cell[]? cells;

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;
            foreach (var item in Children)
            {
                var w = item.DesiredSize.Width;
                if (item is FrameworkElement e && !double.IsNaN(e.Width))
                    w = e.Width;
                item.Arrange(new(x, 0, item.DesiredSize.Width, item.DesiredSize.Height));
                x += w+ extraPadding;
            }
            return new Size(Math.Max(finalSize.Width, x), finalSize.Height);
        }
        double extraPadding = 6;
        protected override Size MeasureOverride(Size availableSize)
        {
            var size = base.MeasureOverride(availableSize);
            var maxWidth = size.Width;
            if (availableSize.Width == double.PositiveInfinity)
            {
                var parent = this.Parent;
                while(parent is FrameworkElement e && (double.IsNaN(e.ActualWidth) || e.ActualWidth == 0))
                {
                    parent = e.Parent;
                }
                if(parent is FrameworkElement fe && !double.IsNaN(fe.ActualWidth) && fe.ActualWidth > 0)
                {
                    maxWidth = fe.ActualWidth;
                }
            }
            if (Columns is not null)
            {
                var width = Math.Max(maxWidth, Columns.Sum(s => s.Width) + extraPadding * Columns.Count);
                return new Size(width, availableSize.Height);
            }
            return size;
        }

        private TableColumnCollection? _columnSizes;

        internal TableColumnCollection? Columns
        {
            get { return _columnSizes; }
            set
            {
                if (_columnSizes != value)
                {
                    _columnSizes = value;
                    OnFieldsPropertyChanged(value);
                }
            }
        }

        private string GetDisplayValue(Field field)
        {
            if (field is null || Feature is null || !Feature.Attributes.ContainsKey(field.Name))
                return string.Empty;
            var value = Feature.Attributes[field.Name];
            if (field.Domain is CodedValueDomain cvd)
            {
                value = cvd.CodedValues.FirstOrDefault(c => c.Code == value)?.Name ?? value;
            }
            return value?.ToString() ?? string.Empty;
        }

        private void OnFieldsPropertyChanged(TableColumnCollection? newValue)
        {
            this.Children.Clear();
            cells = null;
            if (newValue is not null)
            {
                cells = new Cell[newValue.Count];
                int i = 0;
                foreach (var field in newValue)
                {
                    if (field is FeatureAttibuteColumn column)
                    {
                        var textBlock = new TextCell
                        {
                            Text = GetDisplayValue(column.Field),
                            Tag = column.Field,
                            Column = column,
                            Margin = new Thickness(2),
                            HorizontalTextAlignment = column.Field.FieldType == FieldType.Text ? TextAlignment.Left : TextAlignment.Right,
                        };
                        this.Children.Add(textBlock);
                        cells[i++] = textBlock;
                    }
                    else if (field is ActionColumn ac)
                    {
                        ActionCell cell = new ActionCell() { Icon = new SymbolIconSource() { Symbol = Symbol.Edit }, Tooltip = ac.Header };
                        cell.Invoked += (s, e) => // TODO: This should be weak or cleaned up on clear
                        {
                            if (Feature is not null)
                                ac.Invoke(s, Feature);
                        };
                        this.Children.Add(cell);
                        cells[i++] = cell;
                    }
                }
            }
        }
    }
}
