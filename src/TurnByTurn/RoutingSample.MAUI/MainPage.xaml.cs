

using Esri.ArcGISRuntime.Security;
using RoutingSample.ViewModels;

namespace RoutingSample.MAUI
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _mainViewModel;

        public MainPage()
        {
            InitializeComponent();
            _mainViewModel = new MainViewModel();
            BindingContext = _mainViewModel;
        }

        protected override async void OnAppearing()
        {
            try
            {
                PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status == PermissionStatus.Granted)
                {
                    await mapView.LocationDisplay.DataSource.StartAsync();
                }
            }
            catch (Exception)
            {
                // Ignore
            }

            _mainViewModel.LocationDisplay = mapView.LocationDisplay;
            _mainViewModel.LocationDisplay.NavigationPointHeightFactor = 0.5;

            base.OnAppearing();
        }

        private async void ButtonSimulation_Clicked(object sender, EventArgs e)
        {
            // Prompt user for a destination
            var destination = await DisplayPromptAsync(
                "Navigation",
                "Enter your destination:",
                accept: "Go",
                initialValue: _mainViewModel.Address ?? string.Empty);

            if (destination != null)
            {
                _mainViewModel.Address = destination;
                if (_mainViewModel.NavigateCommand.CanExecute(null))
                    _mainViewModel.NavigateCommand.Execute(null);
            }
        }
    }
}