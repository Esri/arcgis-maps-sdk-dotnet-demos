using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using OfflineWorkflowsSample.Infrastructure;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using OfflineWorkflowsSample.Infrastructure.ViewServices;

namespace OfflineWorkflowsSample.GenerateMapArea
{
    public class GenerateMapAreaViewModel : BaseViewModel
    {
        private const string EnvelopeToTakeOffline = "{\"xmin\":-9811025.6479785144,\"ymin\":5127976.7933954755,\"xmax\":-9809970.6486553717,\"ymax\":5128752.6489288369,\"spatialReference\":{\"wkid\":102100,\"latestWkid\":3857}}";

        private IReadOnlyList<LevelOfDetail> _levelsOfDetail;
        private DelegateCommand _generateMapAreaCommand;
        private DelegateCommand _navigateToMapAreaCommand;
        private DelegateCommand _openMapFileCommand;
        private GenerateOfflineMapJob _job;
        private MainViewModel _mainVM;
        public override Map Map
        {
            get => _mainVM.Map;
            set => _mainVM.Map = value;
        }

        public override bool IsBusy { get => _mainVM.IsBusy;
            set { _mainVM.IsBusy = value; }
        }
        public override string IsBusyText
        {
            get => _mainVM.IsBusyText;
            set { _mainVM.IsBusyText = value; }
        }
        public override string ProgressPercentage
        {
            get => _mainVM.ProgressPercentage;
            set { _mainVM.ProgressPercentage = value; }
        }

        public GenerateMapAreaViewModel(MainViewModel parent)
        {
            _mainVM = parent;
            _generateMapAreaCommand = new DelegateCommand(GenerateMapArea);
            _navigateToMapAreaCommand = new DelegateCommand(NavigateToMapArea);
            _openMapFileCommand = new DelegateCommand(RevealInExplorer, () => !InOnlineMode);
        }

        public override MapViewService MapViewService => _mainVM.MapViewService;

        private async void NavigateToMapArea()
        {
            try
            {
                await MapViewService.SetViewpointAsync(new Viewpoint(Envelope.FromJson(EnvelopeToTakeOffline)));
            }
            catch (Exception ex)
            {
                _mainVM.ShowMessage(ex.Message);
            }
        }

        private async void GenerateMapArea()
        {
            try
            {
                IsBusy = true;
                IsBusyText = "Generating an offline map...";

                var offlineDataFolder = OfflineDataStorageHelper.GetDataFolderForMap(Map);
                
                // If temporary data folder exists remove it
                if (Directory.Exists(offlineDataFolder))
                    Directory.Delete(offlineDataFolder, true);
                // If temporary data folder doesn't exists, create it
                if (!Directory.Exists(offlineDataFolder))
                    Directory.CreateDirectory(offlineDataFolder);

                // Get area of interest as a envelope from the map view
                var areaOfInterest = 
                    MapViewService.GetCurrentViewpoint(ViewpointType.BoundingGeometry)
                        .TargetGeometry as Envelope;

                // Step 1 : Create task with the map that is taken offline
                var task = await OfflineMapTask.CreateAsync(Map);

                // Step 2 : Create parameters based on selections
                var parameters = await task.CreateDefaultGenerateOfflineMapParametersAsync(
                   areaOfInterest);
                parameters.MaxScale = _levelsOfDetail[SelectedLevelOfDetail].Scale;
                parameters.MinScale = 0; 
                parameters.IncludeBasemap = IncludeBasemap;
                parameters.ReturnLayerAttachmentOption = ReturnLayerAttachmentOption.None;

                // Step 3 : Create job that does the work asyncronously and 
                //          set progress indication
                _job = task.GenerateOfflineMap(parameters, offlineDataFolder);
                _job.ProgressChanged += ProgressChanged;

                // Step 4 : Run the job and wait the results
                var results = await _job.GetResultAsync();

                if (results.HasErrors)
                {
                    string errorString = "";
                    // If one or more layers fails, layer errors are populated with corresponding errors.
                    foreach (var layerError in results.LayerErrors)
                        errorString += $"Error occurred on {layerError.Key.Name} : {layerError.Value.Message}\n";
                    foreach (var tableError in results.TableErrors)
                        errorString += $"Error occurred on {tableError.Key.TableName} : {tableError.Value.Message}\n";
                    _mainVM.ShowMessage(errorString);
                }
                
                // Step 5 : Use results
                Map = results.OfflineMap;

                InOnlineMode = false;
                _openMapFileCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _mainVM.ShowMessage(ex.Message);
            }
            finally
            {
                IsBusy = false;
                IsBusyText = string.Empty;
            }
        }

        private async void ProgressChanged(object sender, EventArgs e)
        {
            try
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    var generateOfflineMapJob = sender as GenerateOfflineMapJob;
                    ProgressPercentage = generateOfflineMapJob.Progress.ToString() + "%";
                });
            }
            catch (Exception){ }
        }

        public ICommand GenerateMapAreaCommand => _generateMapAreaCommand;
        public ICommand NavigateToMapAreaCommand => _navigateToMapAreaCommand;
        public ICommand OpenMapFileCommand => _openMapFileCommand;

        private bool _includeBasemap = true;
        public bool IncludeBasemap
        {
            get { return _includeBasemap; }
            set { SetProperty(ref _includeBasemap, value); UpdateMap(); }
        }

        private int _maximumLevelOfDetail;
        public int MaximumLevelOfDetail
        {
            get { return _maximumLevelOfDetail; }
            set { SetProperty(ref _maximumLevelOfDetail, value); }
        }

        private string _selectedMaximumScale;
        public string SelectedMaximumScale
        {
            get { return _selectedMaximumScale; }
            set { SetProperty(ref _selectedMaximumScale, value); }
        }

        private string _selectedMinimumScale;
        public string SelectedMinimumScale
        {
            get { return _selectedMinimumScale; }
            set { SetProperty(ref _selectedMinimumScale, value); }
        }

        private int _selectedLevelOfDetail;
        public int SelectedLevelOfDetail
        {
            get { return _selectedLevelOfDetail; }
            set { SetProperty(ref _selectedLevelOfDetail, value); UpdateScales(); }
        }

        private bool _inOnlineMode = true;
        public bool InOnlineMode
        {
            get { return _inOnlineMode; }
            set { SetProperty(ref _inOnlineMode, value); }
        }

        private void UpdateScales()
        {
            if (_levelsOfDetail.Any())
            {
                SelectedMinimumScale = _levelsOfDetail.First().Scale.ToString("F0");
                SelectedMaximumScale = _levelsOfDetail[SelectedLevelOfDetail].Scale.ToString("F0");
            }
        }

        private void UpdateMap()
        {
            if (!IncludeBasemap)
            {
                foreach (var basemapLayer in _mainVM.Map.Basemap.BaseLayers)
                {
                    basemapLayer.IsVisible = false;
                }
                foreach (var referenceLayer in _mainVM.Map.Basemap.ReferenceLayers)
                {
                    referenceLayer.IsVisible = false;
                }
            }
            else
            {
                foreach (var basemapLayer in _mainVM.Map.Basemap.BaseLayers)
                {
                    basemapLayer.IsVisible = true;
                }
                foreach (var referenceLayer in _mainVM.Map.Basemap.ReferenceLayers)
                {
                    referenceLayer.IsVisible = true;
                }
            }
        }
        
        public async Task Initialize()
        {
            try
            {
                IsBusy = true;
                IsBusyText = "Loading map...";

                await Map.LoadAsync();

                if (Map.Item is LocalItem)
                {
                    InOnlineMode = true;
                }

                try
                {
                    // Set the scale information to the UI
                    var basemapLayer = Map.AllLayers.OfType<ArcGISTiledLayer>().FirstOrDefault();
                    if (basemapLayer != null)
                        _levelsOfDetail = basemapLayer.ServiceInfo.TileInfo.LevelsOfDetail;
                    else
                    {
                        var vectorBasemapLayer = Map.AllLayers.OfType<ArcGISVectorTiledLayer>().First();
                        _levelsOfDetail = vectorBasemapLayer.SourceInfo.LevelsOfDetail;
                    }
                    MaximumLevelOfDetail = _levelsOfDetail.Max(x => x.Level);
                }
                catch (Exception)
                {
                    _levelsOfDetail = new List<LevelOfDetail>();
                }
                
                SelectedLevelOfDetail = 17;

            }
            catch (Exception ex)
            {
                _mainVM.ShowMessage(ex.Message);
            }
            finally
            {
                IsBusy = false;
                IsBusyText = string.Empty;
            }
        }

        private async void RevealInExplorer()
        {
            try
            {
                string path = OfflineDataStorageHelper.GetDataFolder();
                path = Path.Combine(path, ((LocalItem) Map.Item).OriginalPortalItemId);
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                await Launcher.LaunchFolderAsync(folder);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                _mainVM.ShowMessage($"Couldn't open folder - {e.Message}");
            }
        }
    }
}
