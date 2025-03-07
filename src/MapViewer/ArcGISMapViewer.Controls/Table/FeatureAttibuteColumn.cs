using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace ArcGISMapViewer.Controls
{
    public partial class TableColumnCollection : ObservableCollection<TableColumn>
    {

        protected override void InsertItem(int index, TableColumn item)
        {
            item.ActualWidthChanged += TableColumn_ActualWidthChanged;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            var item = this[index];
            item.ActualWidthChanged -= TableColumn_ActualWidthChanged;
        }

        private void TableColumn_ActualWidthChanged(object? sender, EventArgs e)
        {
            TotalWidthChanged?.Invoke(this, EventArgs.Empty);
        }

        internal event EventHandler? TotalWidthChanged;
    }

    public partial class TableColumn : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// The maximum width recorded by any field content - used for auto-sizing. Value gets updated as text cells gets measured while rendering/scrolling
        /// </summary>
        internal double DesiredSize { get; set; }

        /// <summary>
        /// The width explicitly set by the user by dragging column
        /// </summary>
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            internal set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register(nameof(Width), typeof(double), typeof(FeatureAttibuteColumn), new PropertyMetadata(double.NaN, OnWidthPropertyChanged));

        private static void OnWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TableColumn table)
            {
                var newValue = (double)e.NewValue;
                if (double.IsNaN(newValue))
                    table.ActualWidth = 150;
                else
                    table.ActualWidth = newValue;
            }
        }

        /// <summary>
        /// The actual rendered width of the column which will be the <see cref="Width"/>, or if not set, determined by default sizing and available space
        /// </summary>
        public double ActualWidth
        {
            get { return (double)GetValue(ActualWidthProperty); }
            set { SetValue(ActualWidthProperty, value); }
        }

        public static readonly DependencyProperty ActualWidthProperty =
            DependencyProperty.Register(nameof(ActualWidth), typeof(double), typeof(FeatureAttibuteColumn), new PropertyMetadata(0d, OnActualWidthPropertyChanged));

        private static void OnActualWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TableColumn)?.ActualWidthChanged?.Invoke(d, EventArgs.Empty);
            (d as TableColumn)?.PropertyChanged?.Invoke(d, new PropertyChangedEventArgs(nameof(ActualWidth)));
        }

        public virtual string? Header { get; set; }

        public override string ToString() => Header ?? string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the column can be resized by the user
        /// </summary>
        public bool IsResizeable
        {
            get { return (bool)GetValue(IsResizeableProperty); }
            set { SetValue(IsResizeableProperty, value); }
        }

        public static readonly DependencyProperty IsResizeableProperty =
            DependencyProperty.Register("IsResizeable", typeof(bool), typeof(TableColumn), new PropertyMetadata(true));

        internal event EventHandler? ActualWidthChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public partial class FeatureAttibuteColumn : TableColumn
    {
        public FeatureAttibuteColumn(Esri.ArcGISRuntime.Data.Field field)
        {
            Field = field;
            Header = field.Alias ?? field.Name;
            FieldName = field.Name;
        }
        public string FieldName { get; private set; }

        public Esri.ArcGISRuntime.Data.Field Field { get; }

        public override string ToString() => Header ?? FieldName;
    }
    public partial class ActionColumn : TableColumn
    {
        public ActionColumn()
        {
            Width = 32;
            IsResizeable = false;
        }
        public string? Tooltip { get; set; }
        public Microsoft.UI.Xaml.Controls.IconSource? Icon { get; set; }
        public override string ToString() => Header ?? string.Empty;
        public event EventHandler<Esri.ArcGISRuntime.Data.Feature>? Invoked;
        internal void Invoke(object? source, Esri.ArcGISRuntime.Data.Feature feature)
        {
            Invoked?.Invoke(source, feature);
        }
    }
}
