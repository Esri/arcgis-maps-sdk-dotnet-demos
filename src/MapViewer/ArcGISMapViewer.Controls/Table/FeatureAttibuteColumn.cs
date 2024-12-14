using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace ArcGISMapViewer.Controls
{
    public partial class TableColumnCollection : ObservableCollection<TableColumn> { }

    public partial class TableColumn : DependencyObject
    {
        internal double MaxWidth { get; set; }
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register(nameof(Width), typeof(double), typeof(FeatureAttibuteColumn), new PropertyMetadata(double.NaN));

        public virtual string? Header { get; set; }

        public override string ToString() => Header ?? string.Empty;

        public bool IsResizeable
        {
            get { return (bool)GetValue(IsResizeableProperty); }
            set { SetValue(IsResizeableProperty, value); }
        }

        public static readonly DependencyProperty IsResizeableProperty =
            DependencyProperty.Register("IsResizeable", typeof(bool), typeof(TableColumn), new PropertyMetadata(true));
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
