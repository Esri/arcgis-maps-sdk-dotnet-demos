using Esri.ArcGISRuntime.Ogc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KmlViewer
{
    public class KmlNodeTemplateSelector : DataTemplateSelector
    {
        public KmlNodeTemplateSelector()
        {
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is Esri.ArcGISRuntime.Mapping.KmlLayer)
                return KmlLayerTemplate;
            if (item is KmlNetworkLink link && link.ListItemType != KmlListItemType.CheckHideChildren)
                return NetworkLinkTemplate;
            if (item is KmlContainer cont && cont.ListItemType != KmlListItemType.CheckHideChildren)
                return FolderTemplate;
            if (item is KmlPlacemark)
                return PlacemarkTemplate;
            return NodeTemplate;
            return base.SelectTemplateCore(item);
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return base.SelectTemplateCore(item, container);
        }

        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate NetworkLinkTemplate { get; set; }
        public DataTemplate PlacemarkTemplate { get; set; }
        public DataTemplate KmlLayerTemplate { get; set; }
        public DataTemplate NodeTemplate { get; set; }
    }
}
