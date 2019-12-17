using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace KmlViewer
{
    public sealed partial class TableOfContents : UserControl
    {
        public TableOfContents()
        {
            this.InitializeComponent();
        }

        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(TableOfContents), new PropertyMetadata(null));

        private void OnItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            ItemInvoked?.Invoke(this, args);
        }

        public event EventHandler<TreeViewItemInvokedEventArgs> ItemInvoked;

        private void TreeView_ContextRequested(UIElement sender, ContextRequestedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            if (datacontext != null && (sender as FrameworkElement).DataContext != datacontext)
            {
                var args = new ContextMenuRequestedEventArgs(datacontext);
                ContextMenuRequested?.Invoke(this, args);
                if (args.MenuItems != null && args.MenuItems.Count > 0)
                {
                    var menu = new MenuFlyout();
                    foreach (var item in args.MenuItems)
                        menu.Items.Add(item);
                    if (e.TryGetPosition(e.OriginalSource as UIElement, out Point p))
                        menu.ShowAt(e.OriginalSource as UIElement, p);
                    else
                        menu.ShowAt(e.OriginalSource as UIElement, new FlyoutShowOptions() { Placement = FlyoutPlacementMode.Auto });
                    e.Handled = true;
                }
            }
        }

        public event EventHandler<ContextMenuRequestedEventArgs> ContextMenuRequested;
    }

    public class ContextMenuRequestedEventArgs : EventArgs
    {
        internal ContextMenuRequestedEventArgs(object node)
        {
            TreeViewNode = node;
        }
        public object TreeViewNode { get; }
        public IList<MenuFlyoutItemBase> MenuItems { get; set; } = new List<MenuFlyoutItemBase>();
    }

    public class TocTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is ILayerContent ilc && !ilc.ShowInLegend)
                return null;
            if (item is KmlLayer)
                return KmlLayerTemplate;
            if (item is Layer l)
                return LayerTemplate;

            if (item is KmlNetworkLink link && link.ListItemType != KmlListItemType.CheckHideChildren)
                return KmlNetworkLinkTemplate;
            if (item is KmlContainer cont && cont.ListItemType != KmlListItemType.CheckHideChildren)
                return KmlFolderTemplate;
            if (item is KmlPlacemark)
                return KmlPlacemarkTemplate;
            if (item is KmlTour)
                return KmlTourTemplate;
            if (item is KmlNode)
                return KmlNodeTemplate;
            return base.SelectTemplateCore(item);
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => base.SelectTemplateCore(item, container);

        public DataTemplate KmlFolderTemplate { get; set; }
        public DataTemplate KmlNetworkLinkTemplate { get; set; }
        public DataTemplate KmlPlacemarkTemplate { get; set; }
        public DataTemplate KmlLayerTemplate { get; set; }
        public DataTemplate KmlNodeTemplate { get; set; }
        public DataTemplate KmlTourTemplate { get; set; }
        public DataTemplate LayerTemplate { get; set; }
    }

    public class TocIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is KmlLayer)
                return new Uri("ms-appx:///Icons/kml_layer.png");
            if (value is ArcGISSceneLayer)
                return new Uri("ms-appx:///Icons/scene_layer.png");
            if (value is Layer)
                return new Uri("ms-appx:///Icons/map_service.png");
            if (value is KmlContainer)
                return new Uri("ms-appx:///Icons/kml_folder.png");
            if (value is KmlNetworkLink)
                return new Uri("ms-appx:///Icons/kml_networklink.png");
            if (value is KmlGroundOverlay)
                return new Uri("ms-appx:///Icons/kml_folder.png");
            if (value is KmlPlacemark pl)
            {
                if (pl.UxIcon?.Source != null)
                    return pl.UxIcon.Source;
                //pl.UxIcon.ToImageSourceAsync();
                return new Uri("ms-appx:///Icons/kml_model.png");
            }
            return new Uri("ms-appx:///Icons/map_service.png"); //fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class InProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is KmlRefreshStatus status)
            {
                return status == KmlRefreshStatus.InProgress;
            }
            if (value is Esri.ArcGISRuntime.LoadStatus lstatus)
                return lstatus == Esri.ArcGISRuntime.LoadStatus.Loading;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
