using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using OfflineWorkflowsSample.Infrastructure;
using OfflineWorkflowsSample.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using OfflineWorkflowsSample.Infrastructure.ViewServices;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

namespace OfflineWorkflowsSample.DownloadMapArea
{
    public class DownloadMapAreaViewModel : ViewModelBase
    {
        private GraphicsOverlay _areasOverlay;
        private IWindowService _windowService;
        private MapViewService _mapViewService;
        
        #region Preplanned map area code
        
        private async Task ConfigureMapAndAreas()
        {
            _areasOverlay = new GraphicsOverlay()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleFillSymbol(
                        SimpleFillSymbolStyle.Solid,
                        Color.FromArgb(0x4C, 0x08, 0x08, 0x08),
                        new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Yellow, 1))
                }
            };

            // Load map from portal
            await Map.LoadAsync();

            if (IsMapOnline)
            {
                // Create new task to 
                var offlineMapTask = await OfflineMapTask.CreateAsync(Map);

                // Get list of areas
                var preplannedMapAreas = await offlineMapTask.GetPreplannedMapAreasAsync();

                // Create UI from the areas
                foreach (var preplannedMapArea in preplannedMapAreas.OrderBy(x => x.PortalItem.Title))
                {
                    // Load area to get the metadata 
                    await preplannedMapArea.LoadAsync();
                    // Using a custom model for easier visualization
                    var model = new MapAreaModel(preplannedMapArea);
                    MapAreas.Add(model);
                    // Graphic that shows the area in the map
                    var graphic = new Graphic(preplannedMapArea.AreaOfInterest);
                    graphic.Attributes.Add("Name", preplannedMapArea.PortalItem.Title);
                    _areasOverlay.Graphics.Add(graphic);
                }

                // Show the overlays on the map.
                _mapViewService.AddGraphicsOverlay(_areasOverlay);
            }
        }

        private async void DownloadMapArea()
        {
            try
            {
                _windowService.SetBusyMessage("Downloading selected area...");
                _windowService.SetBusy(true);

                var offlineDataFolder = Path.Combine(OfflineDataStorageHelper.GetDataFolderForMap(Map));

                // If temporary data folder exists remove it
                if (Directory.Exists(offlineDataFolder))
                    Directory.Delete(offlineDataFolder, true);
                // If temporary data folder doesn't exists, create it
                if (!Directory.Exists(offlineDataFolder))
                    Directory.CreateDirectory(offlineDataFolder);

                // Step 1 Create task that is used to access map information and download areas
                var task = await OfflineMapTask.CreateAsync(Map);

                // Step 2 Create job that handles the download and provides status information 
                // about the progress
                var job = task.DownloadPreplannedOfflineMap(SelectedMapArea.GetArea, offlineDataFolder);
                job.ProgressChanged += async (s, e) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        var generateOfflineMapJob = s as DownloadPreplannedOfflineMapJob;
                        _windowService.SetBusyMessage($"Downloading offline map... {generateOfflineMapJob.Progress}%");
                    });
                };

                // Step 3 Run the job and wait the results
                var results = await job.GetResultAsync();

                // Step 4 Check errors 
                if (results.HasErrors)
                {
                    // If one or more layers fails, layer errors are populated with corresponding errors.
                    foreach (var layerError in results.LayerErrors)
                    {
                        Debug.WriteLine($"Error occurred on {layerError.Key.Name} : {layerError.Value.Message}");
                    }

                    foreach (var tableError in results.TableErrors)
                    {
                        Debug.WriteLine($"Error occurred on {tableError.Key.TableName} : {tableError.Value.Message}");
                    }
                }

                // Step 5 Set offline map to use
                Map = results.OfflineMap;
                _areasOverlay.Graphics.Clear();
            }
            catch (Exception ex)
            {
                await _windowService.ShowAlertAsync(ex.Message, "Couldn't download map area");
            }
            finally
            {
                RefreshCommands();
                _windowService.SetBusy(false);
            }
        }

        private async void SyncMapArea(string parameter)
        {
            try
            {
                _windowService.SetBusy(true);
                var synchronizationMode = SyncDirection.Bidirectional;
                switch (parameter)
                {
                    case "Download":
                        synchronizationMode = SyncDirection.Download;
                        _windowService.SetBusyMessage("Getting latest updates...");
                        break;
                    case "Upload":
                        synchronizationMode = SyncDirection.Upload;
                        _windowService.SetBusyMessage("Pushing local changes...");
                        break;
                    default:
                        synchronizationMode = SyncDirection.Bidirectional;
                        _windowService.SetBusyMessage("Synchronizing features...");
                        break;
                }

                // Create task that is used to synchronize the offline map
                var task = await OfflineMapSyncTask.CreateAsync(Map);
                // Create parameters 
                var parameters = new OfflineMapSyncParameters()
                {
                    SyncDirection = synchronizationMode
                };

                // Create job that does the work asynchronously
                var job = task.SyncOfflineMap(parameters);
                job.ProgressChanged += async (s, e) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        var offlineMapSyncJob = s as OfflineMapSyncJob;
                        _windowService.SetBusyMessage($"Syncing map area...{offlineMapSyncJob.Progress}%");
                    });
                };

                // Run the job and wait the results
                var results = await job.GetResultAsync();
                if (results.HasErrors)
                {
                    // handle nicely
                }

                foreach (var message in job.Messages.Select(x => x.Message))
                {
                    Debug.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                await _windowService.ShowAlertAsync(ex.Message, "Couldn't sync map area");
            }
            finally
            {
                RefreshCommands();
                _windowService.SetBusy(false);
            }
        }

        #endregion Preplanned map area code

        #region properties

        private Map _map;
        private MapAreaModel _selectedMapArea;
        private ObservableCollection<MapAreaModel> _mapAreas = new ObservableCollection<MapAreaModel>();

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

        public bool IsMapOnline
        {
            get => _map.Item is PortalItem;
        }
        
        public MapAreaModel SelectedMapArea
        {
            get => _selectedMapArea;
            set
            {
                SetProperty(ref _selectedMapArea, value);
                ZoomToSelectedMapArea();
                RefreshCommands();
            }
        }
        
        public ObservableCollection<MapAreaModel> MapAreas
        {
            get => _mapAreas;
            set => SetProperty(ref _mapAreas, value);
        }

        #endregion properties

        #region  Misc. Overhead

        public DownloadMapAreaViewModel()
        {
            _downloadMapAreaCommand = new DelegateCommand(DownloadMapArea, CanDownloadMapArea);
            _syncMapAreaCommand = new DelegateCommand<string>(SyncMapArea, CanSyncMapArea);
            _openMapFileCommand = new DelegateCommand(RevealInExplorer, () => !IsMapOnline);
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

                await ConfigureMapAndAreas();
            }
            catch (Exception ex)
            {
                await _windowService.ShowAlertAsync(ex.Message, "Error setting up map areas");
            }
            finally
            {
                _windowService.SetBusy(false);
            }
        }
        
        private async void ZoomToSelectedMapArea()
        {
            _areasOverlay.ClearSelection();
            var selectedGraphic = _areasOverlay.Graphics.FirstOrDefault(x => x.Attributes["Name"].ToString() == SelectedMapArea.Title);
            if (selectedGraphic != null)
            {
                selectedGraphic.IsSelected = true;
            }

            await _mapViewService.SetViewpointGeometryAsync(selectedGraphic.Geometry, 20d);
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
                await _windowService.ShowAlertAsync(e.Message, $"Couldn't open folder");
            }
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);

            // Hide the extent overlays when navigating away.
            _mapViewService.ClearOverlays();
        }

        #endregion Misc. Overhead
        
        #region Commands
        public ICommand DownloadMapAreaCommand => _downloadMapAreaCommand;
        public ICommand SyncMapAreaCommand => _syncMapAreaCommand;
        public ICommand OpenMapFileCommand => _openMapFileCommand;
        
        private DelegateCommand _downloadMapAreaCommand;
        private DelegateCommand<string> _syncMapAreaCommand;
        private DelegateCommand _openMapFileCommand;

        private void RefreshCommands()
        {
            _downloadMapAreaCommand.RaiseCanExecuteChanged();
            _syncMapAreaCommand.RaiseCanExecuteChanged();
            _openMapFileCommand.RaiseCanExecuteChanged();
        }

        private bool CanDownloadMapArea()
        {
            return SelectedMapArea != null && IsMapOnline;
        }

        private bool CanSyncMapArea(string parameter)
        {
            return IsMapOnline == false;
        }

        #endregion Commands

        #region Allow page to update the map

        public delegate void MapChangedHandler(object sender, Map map);

        public event MapChangedHandler MapChanged;

        private void RaiseMapChanged()
        {
            MapChanged?.Invoke(this, Map);
        }

        #endregion Allow page to update the map
    }
}