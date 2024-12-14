using System;
using System.Collections.Generic;
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
            WeakReferenceMessenger.Default.Register<ShowFeatureTable>(this, (r, m) => OnShowFeatureTable(m.Table, m.FeatureQuery));
            tableView.FeatureActionInvoked += TableViewFeature_Clicked;
        }

        private void OnShowFeatureTable(FeatureTable? table, FeatureQueryResult featureQuery)
        {
            this.Visibility = table != null ? Visibility.Visible : Visibility.Collapsed;
            tableView.Table = table;
        }

        public class ShowFeatureTable
        {
            public ShowFeatureTable(FeatureQueryResult result, FeatureTable table) : this(table)
            {
                FeatureQuery = result;
            }

            public ShowFeatureTable(FeatureTable table)
            {
                Table = table;
            }

            public FeatureQueryResult FeatureQuery { get; }
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

        public void TableViewFeature_Clicked(object sender, Feature feature)
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
            flyout.ShowAt(sender as FrameworkElement);
            
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
    }
}
