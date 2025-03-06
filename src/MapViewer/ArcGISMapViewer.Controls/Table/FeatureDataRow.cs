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

                if (item is TextCell cell && cell.Column is not null)
                {
                        w = cell.Column.ActualWidth;
                    var fe = VisualTreeHelper.GetChild(cell, 0) as FrameworkElement;
                    if (!double.IsNaN(cell.TextWidth)) // Update desired max size
                        cell.Column.DesiredSize = Math.Max(cell.Column.DesiredSize, cell.TextWidth);
                }
                item.Arrange(new(x - extraPadding / 2, 0, w + extraPadding, item.DesiredSize.Height));
                x += w+ extraPadding;
            }
            return new Size(Math.Max(finalSize.Width, x), finalSize.Height);
        }

        private double extraPadding = 6;

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = base.MeasureOverride(availableSize);
            var maxWidth = size.Width;
            if (Columns is not null && Columns.Any(s => double.IsNaN(s.Width)))
            {
                // If have unlimited with which we usually have in a horizontal scroller, find the featuretableview,
                // and get the width for good intial auto-sizing of the last column
                if (availableSize.Width == double.PositiveInfinity)
                {
                    var parent = VisualTreeHelper.GetParent(this);
                    while (parent is not FeatureTableView && parent is FrameworkElement e)
                    {
                        parent = VisualTreeHelper.GetParent(parent);
                    }
                    if (parent is FrameworkElement fe && !double.IsNaN(fe.ActualWidth) && fe.ActualWidth > 0)
                    {
                        maxWidth = fe.ActualWidth;
                    }
                }
                double x = 0;
                foreach (var column in Columns)
                {
                    if (double.IsNaN(column.Width))
                    {
                        if (column == Columns.Last())
                            column.ActualWidth = Math.Max(150, maxWidth - x - extraPadding);
                        else
                            column.ActualWidth = 150;
                    }
                    x += column.ActualWidth + extraPadding;
                }
            }
            if (Columns is not null)
            {
                 var width = Math.Max(maxWidth, Columns.Where(s=>!double.IsNaN(s.Width)).Sum(s => s.Width) + extraPadding * Columns.Count);
                width += Math.Max(maxWidth, Columns.Where(s => double.IsNaN(s.Width)).Count() * 150 + extraPadding * Columns.Count);
                return new Size(width, availableSize.Height);
            }
            return new Size(maxWidth, availableSize.Height);
        }

        private TableColumnCollection? _columnSizes;

        internal TableColumnCollection? Columns
        {
            get { return _columnSizes; }
            set
            {
                if (_columnSizes != value)
                {
                    if (_columnSizes is not null)
                    {
                        _columnSizes.TotalWidthChanged -= Value_TotalWidthChanged;
                    }
                    _columnSizes = value;
                    OnFieldsPropertyChanged(value);
                    if (value is not null)
                    {
                        value.TotalWidthChanged += Value_TotalWidthChanged;
                    }
                }
            }
        }

        private void Value_TotalWidthChanged(object? sender, EventArgs e)
        {
            InvalidateMeasure();
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
                            HorizontalAlignment = column.Field.FieldType == FieldType.Text ? HorizontalAlignment.Left : HorizontalAlignment.Right
                        };
                        this.Children.Add(textBlock);
                        cells[i++] = textBlock;
                    }
                    else if (field is ActionColumn ac)
                    {
                        ActionCell cell = new ActionCell() { Icon = ac.Icon , Tooltip = ac.Header };
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
