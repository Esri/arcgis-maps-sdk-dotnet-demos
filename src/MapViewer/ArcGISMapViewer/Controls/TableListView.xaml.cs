using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Toolkit.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISMapViewer.Controls
{
    public sealed partial class TableListView : UserControl
    {
        public TableListView()
        {
            this.InitializeComponent();
        }



        public GeoModel GeoModel
        {
            get { return (GeoModel)GetValue(GeoModelProperty); }
            set { SetValue(GeoModelProperty, value); }
        }

        public static readonly DependencyProperty GeoModelProperty =
            DependencyProperty.Register("GeoModel", typeof(GeoModel), typeof(LayerListView), new PropertyMetadata(null));



        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            var table = (sender as FrameworkElement)?.DataContext as FeatureTable;
            if (table is not null)
            {
                TextBox tb = new TextBox() { Text = table.DisplayName };
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Rename table",
                    Content = tb,
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot
                };
                var result = await dialog.ShowAsync();
                if(result == ContentDialogResult.Primary)
                {
                    table.DisplayName = tb.Text;
                }
            }
        }

        private async void Remove_Click(object sender, RoutedEventArgs e)
        {
            var table = (sender as FrameworkElement)?.DataContext as FeatureTable;
            if (table is not null)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Remove table?",
                    PrimaryButtonText = "OK",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    if (GeoModel.Tables.Contains(table))
                        GeoModel.Tables.Remove(table);
                }
            }
        }
    }
}
