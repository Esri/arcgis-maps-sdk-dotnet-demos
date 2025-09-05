using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.Messaging;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ArcGISMapViewer.Controls
{
    public sealed partial class TableView : UserControl
    {
        public TableView()
        {
            this.InitializeComponent();
            WeakReferenceMessenger.Default.Register<ShowFeatureTable>(this, (r, m) => OnShowFeatureTable(m.Table, m.WhereClause));
        }

        public ObservableCollection<FeatureTable> Tables { get; } = new ObservableCollection<FeatureTable>();

        private void OnShowFeatureTable(FeatureTable table, string? whereClause)
        {
            if (table is not null)
            {
                if (!Tables.Contains(table))
                    Tables.Add(table);
                TabView.SelectedItem = table;
            }
            if (Tables.Count > 0)
                this.Visibility = Visibility.Visible;
        }

        private void Close_Clicked(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        public class ShowFeatureTable
        {
            public ShowFeatureTable(string whereClause, FeatureTable table) : this(table)
            {
                WhereClause = whereClause;
            }

            public ShowFeatureTable(FeatureTable table)
            {
                Table = table;
            }

            public string? WhereClause { get; }
            public FeatureTable Table { get; }
        }

        public FeatureQueryResult FeatureQuery
        {
            get { return (FeatureQueryResult)GetValue(FeatureQueryProperty); }
            set { SetValue(FeatureQueryProperty, value); }
        }

        public static readonly DependencyProperty FeatureQueryProperty =
            DependencyProperty.Register("FeatureQuery", typeof(FeatureQueryResult), typeof(FeatureTableView), new PropertyMetadata(null, (s, e) => ((TableView)s).OnFeatureQueryPropertyChanged(e.NewValue as FeatureQueryResult)));

        private void OnFeatureQueryPropertyChanged(FeatureQueryResult? newResult)
        {
            // TableView.ItemsSource = null!;
            // TableView.Columns.Clear();
            if (newResult is null) return;
            IValueConverter? converter = new AttributeConverter();
            foreach (var field in newResult.Fields)
            {
                //CommunityToolkit.WinUI.Controls.
               // TableView.Columns.Add(new TableViewTextColumn() { Header = field.Alias ?? field.Name, Binding = new Binding() { Path = new PropertyPath("Attributes"), Converter = converter, ConverterParameter = field } });
            }
            //TableView.ItemsSource = new List<Feature>(newResult);
        }

        public void TableViewFeature_Clicked(object? sender, Feature feature)
        {
            MenuFlyout flyout = new MenuFlyout();
            var edit = new MenuFlyoutItem() { Text = "Edit" };
            edit.Click += (s,e ) => WeakReferenceMessenger.Default.Send(new Views.MapPage.ShowRightPanelMessage(Views.MapPage.ShowRightPanelMessage.PanelId.EditFeature, feature)); ;
            flyout.Items.Add(edit);
            if (feature.FeatureTable!.GeometryType != GeometryType.Unknown)
            {
                var zoomto = new MenuFlyoutItem() { Text = "Zoom to" };
                zoomto.Click += async (s, e) =>
                {
                    if (feature.Geometry is null && feature is ILoadable loadable)
                        try
                        {
                            await loadable.LoadAsync();
                        }
                        catch { }
                    if (feature.Geometry is not null)
                    {
                        (feature.FeatureTable?.Layer as FeatureLayer)?.ClearSelection();
                        (feature.FeatureTable?.Layer as FeatureLayer)?.SelectFeature(feature);
                        WeakReferenceMessenger.Default.Send(new Viewpoint(feature.Geometry));
                    }
                };
                flyout.Items.Add(zoomto);
            }
            flyout.ShowAt(sender as FrameworkElement, new FlyoutShowOptions() {  ShowMode = FlyoutShowMode.Standard });
            
        }

        private partial class AttributeConverter : IValueConverter
        {
            public object? Convert(object? value, Type targetType, object? parameter, string language)
            {
                if(value is Feature feature && parameter is Field field)
                {
                    return feature.Attributes.ContainsKey(field.Name) ? feature.Attributes[field.Name] : null;
                }
                if (value is IDictionary<string,object?> dic && parameter is Field field2)
                {
                    return dic.ContainsKey(field2.Name) ? dic[field2.Name] : null;
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                throw new NotImplementedException();
            }
        }

        private void TabViewItem_CloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Item is FeatureTable table && Tables.Contains(table))
                Tables.Remove(table);
            if (Tables.Count == 0)
                this.Visibility = Visibility.Collapsed;
        }
    }
}
