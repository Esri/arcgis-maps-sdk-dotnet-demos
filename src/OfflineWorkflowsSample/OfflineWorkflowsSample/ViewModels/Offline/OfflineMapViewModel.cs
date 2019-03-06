using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample;
using OfflineWorkflowsSample.DownloadMapArea;
using OfflineWorkflowsSample.GenerateMapArea;
using OfflineWorkflowsSample.Infrastructure;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace OfflineWorkflowSample.ViewModels
{
    public class OfflineMapViewModel : BaseViewModel
    {
        private MainViewModel _mainVM => (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];

        private GenerateMapAreaViewModel _generateMapAreaViewModel;

        public GenerateMapAreaViewModel GenerateMapAreaViewModel
        {
            get { return _generateMapAreaViewModel; }
            set { SetProperty(ref _generateMapAreaViewModel, value); }
        }

        private DownloadMapAreaViewModel _downloadMapAreaViewModel;

        public DownloadMapAreaViewModel DownloadMapAreaViewModel
        {
            get { return _downloadMapAreaViewModel; }
            set { SetProperty(ref _downloadMapAreaViewModel, value); }
        }

        private Map _onlineMap;

        public Map OnlineMap
        {
            get => _onlineMap;
            set => SetProperty(ref _onlineMap, value);
        }

        public async void Initialize(Map map)
        {
            if (map.Item is PortalItem)
            {
                OnlineMap = map;
            }
            else if (map.Item is LocalItem localItem)
            {
                // It appears navigation fails if this takes too long,
                // so moved this into its own function that isn't awaited
                LoadOnlineMapItemForOfflineMap(localItem);
            }

            Map = map;
            GenerateMapAreaViewModel = new GenerateMapAreaViewModel();
            DownloadMapAreaViewModel = new DownloadMapAreaViewModel();

            await Task.WhenAll(
                GenerateMapAreaViewModel.Initialize(map, _mainVM._windowService, MapViewService),
                DownloadMapAreaViewModel.Initialize(map, _mainVM._windowService, MapViewService));

            GenerateMapAreaViewModel.MapChanged += UpdateMap;
            DownloadMapAreaViewModel.MapChanged += UpdateMap;
        }

        private async void LoadOnlineMapItemForOfflineMap(LocalItem localItem)
        {
            try
            {
                PortalItem onlineItem = await PortalItem.CreateAsync(_mainVM.PortalViewModel.Portal, localItem.OriginalPortalItemId);
                OnlineMap = new Map(onlineItem);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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

                if (newMap.Item is PortalItem)
                {
                    OnlineMap = newMap;
                }
            }
        }
    }
}