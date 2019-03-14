using System;
using Esri.ArcGISRuntime.Mapping;
using Prism.Windows.Mvvm;

namespace OfflineWorkflowSample.ViewModels.ItemPages
{
    public class ScenePageViewModel : ViewModelBase
    {
        private PortalItemViewModel _item;
        private Scene _map;

        public Scene Scene
        {
            get => _map;
            private set => SetProperty(ref _map, value);
        }

        public PortalItemViewModel Item
        {
            get => _item;
            private set => SetProperty(ref _item, value);
        }

        public void Initialize(Scene map, PortalItemViewModel item)
        {
            Scene = map;
            Item = item;
        }

        public void Reset()
        {
            Scene.Basemap = new Basemap(); // Should be set to null but there's a bug as of 100.5-daily2354
            Scene.OperationalLayers.Clear();
            Scene = null;
            Item = null;
        }
    }
}