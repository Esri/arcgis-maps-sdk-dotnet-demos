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
using OfflineWorkflowsSample.Infrastructure.ViewServices;
using Prism.Windows.Navigation;

namespace OfflineWorkflowsSample.DownloadMapArea
{
    public class DownloadMapAreaViewModel : BaseViewModel
    {
        private readonly ArcGISPortal _portal;
        private GraphicsOverlay _areasOverlay;
        private DelegateCommand _downloadMapAreaCommand;
        private DelegateCommand<string> _syncMapAreaCommand;
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

        public DownloadMapAreaViewModel(MainViewModel parent)
        {
            _portal = parent.Portal ?? throw new ArgumentNullException(nameof(parent));
            _mainVM = parent;
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

            _downloadMapAreaCommand = new DelegateCommand(DownloadMapArea, CanDownloadMapArea);
            _syncMapAreaCommand = new DelegateCommand<string>(SyncMapArea, CanSyncMapArea);
        }

        public override MapViewService MapViewService => _mainVM.MapViewService;

        private async void DownloadMapArea()
        {
            try
            {
                IsBusy = true;
                IsBusyText = "Downloading selected area...";

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
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                        var generateOfflineMapJob = s as DownloadPreplannedOfflineMapJob;
                        ProgressPercentage = generateOfflineMapJob.Progress.ToString() + "%";
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

                InOnlineMode = false;
                _areasOverlay.Graphics.Clear();
            }
            catch (Exception ex)
            {
                _mainVM.ShowMessage(ex.Message);
            }
            finally
            {
                RefreshCommands();
                IsBusy = false;
                IsBusyText = string.Empty;
            }
        }

        private bool CanDownloadMapArea()
        {
            if (SelectedMapArea != null)
                return true;
            return false;
        }

        public ICommand DownloadMapAreaCommand => _downloadMapAreaCommand;

        // Sync map area
        private async void SyncMapArea(string parameter)
        {
            try
            {
                IsBusy = true;
                var synchronizationMode = SyncDirection.Bidirectional;
                switch (parameter)
                {
                    case "Download":
                        synchronizationMode = SyncDirection.Download;
                        IsBusyText = "Getting latest updates...";
                        break;
                    case "Upload":
                        synchronizationMode = SyncDirection.Upload;
                        IsBusyText = "Pushing local changes...";
                        break;
                    default:
                        synchronizationMode = SyncDirection.Bidirectional;
                        IsBusyText = "Synchronazing features...";
                        break;
                }

                // Create task that is used to synchronize the offline map
                var task = await OfflineMapSyncTask.CreateAsync(Map);
                // Create parameters 
                var parameters = new OfflineMapSyncParameters()
                {
                    SyncDirection = synchronizationMode
                };

                // Create job that does the work asyncronously
                var job = task.SyncOfflineMap(parameters);
                job.ProgressChanged += async (s, e) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                        var offlineMapSyncJob = s as OfflineMapSyncJob;
                        ProgressPercentage = offlineMapSyncJob.Progress.ToString() + "%";
                    });
                };

                // Run the job and wait the results
                var results = await job.GetResultAsync();
                if (results.HasErrors)
                {
                    // handle nicely
                }

                foreach (var message in job.Messages.Select(x =>x.Message))
                {
                    Debug.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                _mainVM.ShowMessage(ex.Message);
            }
            finally
            {
                RefreshCommands();
                IsBusy = false;
                IsBusyText = string.Empty;
            }
        }

        private bool CanSyncMapArea(string parameter)
        {
            if (InOnlineMode == false)
                return true;
            return false;
        }

        public ICommand SyncMapAreaCommand => _syncMapAreaCommand;

        private GraphicsOverlayCollection _graphicsOverlays;
        public GraphicsOverlayCollection GraphicsOverlays
        {
            get { return _graphicsOverlays; }
            set { SetProperty(ref _graphicsOverlays, value); }
        }

        private ObservableCollection<MapAreaModel> _mapAreas = new ObservableCollection<MapAreaModel>();
        public ObservableCollection<MapAreaModel> MapAreas
        {
            get { return _mapAreas; }
            set { SetProperty(ref _mapAreas, value); }
        }

        private MapAreaModel _selectedMapArea;
        public MapAreaModel SelectedMapArea
        {
            get { return _selectedMapArea; }
            set { SetProperty(ref _selectedMapArea, value); UpdateMap(); RefreshCommands(); }
        }

        private async void UpdateMap()
        {
            _areasOverlay.ClearSelection();
            var selectedGraphic = _areasOverlay.Graphics.FirstOrDefault(x => x.Attributes["Name"].ToString() == SelectedMapArea.Title);
            if (selectedGraphic != null)
            {
                selectedGraphic.IsSelected = true;
            }
            await MapViewService.SetViewpointGeometryAsync(selectedGraphic.Geometry, 20d);
        }

        public void RefreshCommands()
        {
            _downloadMapAreaCommand.RaiseCanExecuteChanged();
            _syncMapAreaCommand.RaiseCanExecuteChanged();
        }

        private bool _inOnlineMode = true;
        public bool InOnlineMode
        {
            get { return _inOnlineMode; }
            set { SetProperty(ref _inOnlineMode, value); }
        }
        
        public async Task Initialize()
        {
            try
            {
                IsBusy = true;
                IsBusyText = "Loading map...";

                // Load map from portal
                await Map.LoadAsync();

                if (Map.Item is LocalItem)
                {
                    InOnlineMode = true;
                }
                else
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
                    _mainVM.MapViewService.AddGraphicsOverlay(_areasOverlay);
                }
                
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

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);

            // Hide the extent overlays when navigating away.
            _mainVM.MapViewService.ClearOverlays();
        }
    }
}
