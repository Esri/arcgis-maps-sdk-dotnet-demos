using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Esri.ArcGISRuntime.Mapping;
using OfflineWorkflowsSample;
using OfflineWorkflowSample.ViewModels;

namespace OfflineWorkflowSample.Views
{
    public sealed partial class OfflineMapPage : Page
    {
        private MainViewModel _mainVM = (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];

        public OfflineMapPage()
        {
            InitializeComponent();
        }

        private OfflineMapViewModel ViewModel => (OfflineMapViewModel) Resources[nameof(ViewModel)];

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel.MapViewService = _mainVM.MapViewService;
            ViewModel.Initialize(new Map(_mainVM.SelectedItem.Item), _mainVM.SelectedItem, _mainVM.PortalViewModel.Portal, _mainVM.WindowService);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // Reset the view model to avoid object already owned exceptions.
            ViewModel.Reset();
        }
    }
}