using Esri.ArcGISRuntime.Mapping;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System.Windows.Input;
using OfflineWorkflowsSample.Infrastructure.ViewServices;
using System;

namespace OfflineWorkflowsSample.Infrastructure
{
    public class BaseViewModel : ViewModelBase
    {
        public BaseViewModel()
        {
            _resetViewPointCommand = new DelegateCommand(ResetViewPoint);
            _zoomInCommand = new DelegateCommand(ZoomIn);
            _zoomOutCommand = new DelegateCommand(ZoomOut);
        }

        private async void ResetViewPoint()
        {
            try
            {
                await MapViewService.ResetViewpointAsync();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private readonly DelegateCommand _resetViewPointCommand;
        public ICommand ResetViewPointCommand => _resetViewPointCommand;

        private readonly DelegateCommand _zoomInCommand;
        public ICommand ZoomInCommand => _zoomInCommand;

        private async void ZoomIn()
        {
            try
            {
                var currentScale = MapViewService.MapScale;
                await MapViewService.SetViewpointScaleAsync(currentScale * 0.5);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private readonly DelegateCommand _zoomOutCommand;
        public ICommand ZoomOutCommand => _zoomOutCommand;

        private async void ZoomOut()
        {
            try
            {
                var currentScale = MapViewService.MapScale;
                await MapViewService.SetViewpointScaleAsync(currentScale * 1.5);
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        private MapViewService _mapViewService;

        /// <summary>
        /// Gets the view service that provides access to the MapView
        /// </summary>
        public MapViewService MapViewService
        {
            get => _mapViewService ?? (_mapViewService = new MapViewService());
            set => _mapViewService = value;
        }

        private Map _map;

        public Map Map
        {
            get => _map;
            set => SetProperty(ref _map, value);
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _isBusyText;

        public string IsBusyText
        {
            get => _isBusyText;
            set => SetProperty(ref _isBusyText, value);
        }

        private string _progressPercentage;

        public string ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref _progressPercentage, value);
        }
    }
}