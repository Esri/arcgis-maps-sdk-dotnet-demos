using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using OfflineWorkflowsSample.Infrastructure;
using OfflineWorkflowsSample.Infrastructure.ViewServices;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;

namespace OfflineWorkflowsSample.GenerateMapArea
{
    public class GenerateMapAreaViewModel : ViewModelBase
    {
        private const string EnvelopeToTakeOffline =
            "{\"xmin\":-9811025.6479785144,\"ymin\":5127976.7933954755,\"xmax\":-9809970.6486553717,\"ymax\":5128752.6489288369,\"spatialReference\":{\"wkid\":102100,\"latestWkid\":3857}}";

        private IReadOnlyList<LevelOfDetail> _levelsOfDetail;
        private GenerateOfflineMapJob _job;

        #region Sample code

        private async void GenerateMapArea()
        {
            try
            {
                _windowService.SetBusyMessage("Taking map offline");
                _windowService.SetBusy(true);

                string offlineDataFolder = OfflineDataStorageHelper.GetDataFolderForMap(Map);

                // If temporary data folder exists remove it
                try
                {
                    if (Directory.Exists(offlineDataFolder))
                        Directory.Delete(offlineDataFolder, true);
                }
                catch (Exception)
                {
                    // If folder can't be deleted, open a new one.
                    offlineDataFolder = Path.Combine(offlineDataFolder, DateTime.Now.Ticks.ToString());
                }

                // If temporary data folder doesn't exists, create it
                if (!Directory.Exists(offlineDataFolder))
                    Directory.CreateDirectory(offlineDataFolder);

                // Get area of interest as a envelope from the map view
                var areaOfInterest =
                    _mapViewService.GetCurrentViewpoint(ViewpointType.BoundingGeometry)
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

                // Step 3 : Create job that does the work asynchronously and 
                //          set progress indication
                _job = task.GenerateOfflineMap(parameters, offlineDataFolder);
                _job.ProgressChanged += GenerateOfflineMap_ProgressChanged;

                // Step 4 : Run the job and wait the results
                var results = await _job.GetResultAsync();

                if (results.HasErrors)
                {
                    string errorString = "";
                    // If one or more layers fails, layer errors are populated with corresponding errors.
                    foreach (var (key, value) in results.LayerErrors)
                        errorString += $"Error occurred on {key.Name} : {value.Message}\r\n";
                    foreach (var (key, value) in results.TableErrors)
                        errorString += $"Error occurred on {key.TableName} : {value.Message}\r\n";
                    OfflineDataStorageHelper.FlushLogToDisk(errorString, Map);
                }

                // Step 5 : Use results
                Map = results.OfflineMap;

                RefreshCommands();
            }
            catch (Exception ex)
            {
                await _windowService.ShowAlertAsync(ex.Message, "Error generating offline map");
            }
            finally
            {
                _windowService.SetBusy(false);
            }
        }

        #endregion Sample code

        #region Properties

        private Map _map;
        private bool _includeBasemap = true;
        private int _maximumLevelOfDetail;
        private string _selectedMaximumScale;
        private string _selectedMinimumScale;
        private int _selectedLevelOfDetail;

        public Map Map
        {
            get => _map;
            set
            {
                if (_map != value)
                {
                    SetProperty(ref _map, value);
                    RaisePropertyChanged(nameof(IsMapOnline));
                    RaiseMapChanged();
                    RefreshCommands();
                }
            }
        }

        public bool IncludeBasemap
        {
            get => _includeBasemap;
            set
            {
                SetProperty(ref _includeBasemap, value);
                ToggleIncludeBasemap();
            }
        }

        public int MaximumLevelOfDetail
        {
            get => _maximumLevelOfDetail;
            set => SetProperty(ref _maximumLevelOfDetail, value);
        }

        public string SelectedMaximumScale
        {
            get => _selectedMaximumScale;
            set => SetProperty(ref _selectedMaximumScale, value);
        }

        public string SelectedMinimumScale
        {
            get => _selectedMinimumScale;
            set => SetProperty(ref _selectedMinimumScale, value);
        }

        public int SelectedLevelOfDetail
        {
            get => _selectedLevelOfDetail;
            set
            {
                SetProperty(ref _selectedLevelOfDetail, value);
                UpdateScales();
            }
        }

        public bool IsMapOnline => _map?.Item is PortalItem;

        #endregion Properties

        #region Misc. Overhead

        private IWindowService _windowService;
        private MapViewService _mapViewService;

        public GenerateMapAreaViewModel()
        {
            _generateMapAreaCommand = new DelegateCommand(GenerateMapArea, () => IsMapOnline);
            _navigateToMapAreaCommand = new DelegateCommand(NavigateToMapArea, () => IsMapOnline);
            _openMapFileCommand = new DelegateCommand(RevealInExplorer, () => !IsMapOnline);
            _resetMapCommand = new DelegateCommand(() => RaiseMapChanged(true), () => !IsMapOnline);
        }

        public async Task Initialize(Map map, IWindowService windowService, MapViewService mapViewService)
        {
            _windowService = windowService;
            _mapViewService = mapViewService;
            Map = map;
            try
            {
                _windowService.SetBusyMessage("Loading map...");
                _windowService.SetBusy(true);

                await Map.LoadAsync();

                try
                {
                    // Set the scale information to the UI
                    var basemapLayer = Map.AllLayers.OfType<ImageTiledLayer>().FirstOrDefault();
                    if (basemapLayer != null)
                    {
                        _levelsOfDetail = basemapLayer.TileInfo.LevelsOfDetail;
                        MaximumLevelOfDetail = _levelsOfDetail.Max(x => x.Level);
                        SelectedLevelOfDetail = MaximumLevelOfDetail;
                    }
                    else if (Map.AllLayers.OfType<ArcGISVectorTiledLayer>().Any())
                    {
                        var vectorBasemapLayer = Map.AllLayers.OfType<ArcGISVectorTiledLayer>().First();
                        _levelsOfDetail = vectorBasemapLayer.SourceInfo.LevelsOfDetail;
                        MaximumLevelOfDetail = _levelsOfDetail.Max(x => x.Level);
                        SelectedLevelOfDetail = MaximumLevelOfDetail;
                    }
                    else
                    {
                        _levelsOfDetail = null;
                    }
                    
                }
                catch (Exception)
                {
                    _levelsOfDetail = null;
                }
            }
            catch (Exception ex)
            {
                await _windowService.ShowAlertAsync(ex.Message, "Couldn't load map.");
            }
            finally
            {
                _windowService.SetBusy(false);
            }
        }

        private void ToggleIncludeBasemap()
        {
            if (!IncludeBasemap)
            {
                foreach (var basemapLayer in Map.Basemap.BaseLayers)
                {
                    basemapLayer.IsVisible = false;
                }

                foreach (var referenceLayer in Map.Basemap.ReferenceLayers)
                {
                    referenceLayer.IsVisible = false;
                }
            }
            else
            {
                foreach (var basemapLayer in Map.Basemap.BaseLayers)
                {
                    basemapLayer.IsVisible = true;
                }

                foreach (var referenceLayer in Map.Basemap.ReferenceLayers)
                {
                    referenceLayer.IsVisible = true;
                }
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
                await _windowService.ShowAlertAsync(e.Message, "Couldn't open folder");
            }
        }

        private void UpdateScales()
        {
            if (_levelsOfDetail.Any())
            {
                SelectedMinimumScale = _levelsOfDetail.First().Scale.ToString("F0");
                SelectedMaximumScale = _levelsOfDetail[SelectedLevelOfDetail].Scale.ToString("F0");
            }
        }

        private async void GenerateOfflineMap_ProgressChanged(object sender, EventArgs e)
        {
            try
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var generateOfflineMapJob = sender as GenerateOfflineMapJob;
                    _windowService.SetBusyMessage($"Taking map offline... {generateOfflineMapJob.Progress}%");
                });
            }
            catch (Exception)
            {
                // Ignored.
            }
        }

        private async void NavigateToMapArea()
        {
            try
            {
                await _mapViewService.SetViewpointAsync(new Viewpoint(Geometry.FromJson(EnvelopeToTakeOffline)));
            }
            catch (Exception ex)
            {
                await _windowService.ShowAlertAsync(ex.Message, "Couldn't navigate to map area");
            }
        }

        #endregion Misc. Overhead

        #region Commands

        // Note: commands are configured in the constructor.
        private readonly DelegateCommand _generateMapAreaCommand;
        private readonly DelegateCommand _navigateToMapAreaCommand;
        private readonly DelegateCommand _openMapFileCommand;
        private readonly DelegateCommand _resetMapCommand;

        public ICommand GenerateMapAreaCommand => _generateMapAreaCommand;
        public ICommand NavigateToMapAreaCommand => _navigateToMapAreaCommand;
        public ICommand OpenMapFileCommand => _openMapFileCommand;
        public ICommand ResetMapCommand => _resetMapCommand;

        private void RefreshCommands()
        {
            _generateMapAreaCommand.RaiseCanExecuteChanged();
            _navigateToMapAreaCommand.RaiseCanExecuteChanged();
            _openMapFileCommand.RaiseCanExecuteChanged();
            _resetMapCommand.RaiseCanExecuteChanged();
        }

        #endregion Commands

        #region Allow page to update the map

        public delegate void MapChangedHandler(object sender, Map map);

        public event MapChangedHandler MapChanged;

        private void RaiseMapChanged(bool resetMap = false)
        {
            MapChanged?.Invoke(this, resetMap ? null : Map);
        }

        #endregion Allow page to update the map
    }
}