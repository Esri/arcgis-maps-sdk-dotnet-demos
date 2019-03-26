using Esri.ArcGISRuntime.Mapping;
using Prism.Windows.Mvvm;

namespace OfflineWorkflowSample.ViewModels.ItemPages
{
    public class MapPageViewModel : ViewModelBase
    {
        private PortalItemViewModel _item;
        private Map _map;

        public Map Map
        {
            get => _map;
            private set => SetProperty(ref _map, value);
        }

        public PortalItemViewModel Item
        {
            get => _item;
            private set => SetProperty(ref _item, value);
        }

        public void Initialize(Map map, PortalItemViewModel item)
        {
            Map = map;
            Item = item;
        }

        public void Reset()
        {
            if (Map != null)
            {
                Map.Basemap = null;
                Map.OperationalLayers.Clear();
                Map = null;
            }
            
            Item = null;
        }
    }
}