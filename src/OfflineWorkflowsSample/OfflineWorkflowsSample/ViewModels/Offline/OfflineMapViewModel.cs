using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample;
using OfflineWorkflowsSample.DownloadMapArea;
using OfflineWorkflowsSample.GenerateMapArea;
using OfflineWorkflowsSample.Infrastructure;

namespace OfflineWorkflowSample.ViewModels
{
    public class OfflineMapViewModel : BaseViewModel
    {
        private readonly IWindowService _windowService;

        private DownloadMapAreaViewModel _downloadMapAreaViewModel;

        private GenerateMapAreaViewModel _generateMapAreaViewModel;

        private Map _onlineMap;

        public OfflineMapViewModel(IWindowService windowService, ArcGISPortal portal)
        {
            _windowService = windowService;
            Portal = portal;
        }

        private ArcGISPortal Portal { get; }

        public GenerateMapAreaViewModel GenerateMapAreaViewModel
        {
            get => _generateMapAreaViewModel;
            private set => SetProperty(ref _generateMapAreaViewModel, value);
        }

        public DownloadMapAreaViewModel DownloadMapAreaViewModel
        {
            get => _downloadMapAreaViewModel;
            private set => SetProperty(ref _downloadMapAreaViewModel, value);
        }

        private Map OnlineMap
        {
            get => _onlineMap;
            set => SetProperty(ref _onlineMap, value);
        }

        public async void Initialize(Map map)
        {
            try
            {
                switch (map.Item)
                {
                    case PortalItem _:
                        OnlineMap = map;
                        break;
                    case LocalItem localItem:
                        // Load the online map that the offline map was made from.
                        await LoadOnlineMapItemForOfflineMap(localItem);
                        break;
                }

                Map = map;

                // Configure the view models.
                GenerateMapAreaViewModel = new GenerateMapAreaViewModel();
                DownloadMapAreaViewModel = new DownloadMapAreaViewModel();

                await Task.WhenAll(
                    GenerateMapAreaViewModel.Initialize(map, _windowService, MapViewService),
                    DownloadMapAreaViewModel.Initialize(map, _windowService, MapViewService));

                // Listen for map changes - happens when the map is taken offline.
                GenerateMapAreaViewModel.MapChanged += UpdateMap;
                DownloadMapAreaViewModel.MapChanged += UpdateMap;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private async Task LoadOnlineMapItemForOfflineMap(LocalItem localItem)
        {
            PortalItem onlineItem = await PortalItem.CreateAsync(Portal, localItem.OriginalPortalItemId);
            OnlineMap = new Map(onlineItem);
        }

        private void UpdateMap(object sender, Map newMap)
        {
            if (newMap == null)
            {
                Map oldMap = GenerateMapAreaViewModel.Map;
                Map = OnlineMap;
                GenerateMapAreaViewModel.Map = Map;
                DownloadMapAreaViewModel.Map = Map;
                oldMap.OperationalLayers.Clear();
                oldMap.Tables.Clear();
                // ReSharper disable once RedundantAssignment
                oldMap = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            else
            {
                Map = newMap;
                GenerateMapAreaViewModel.Map = newMap;
                DownloadMapAreaViewModel.Map = newMap;

                if (newMap.Item is PortalItem) OnlineMap = newMap;
            }
        }
    }
}