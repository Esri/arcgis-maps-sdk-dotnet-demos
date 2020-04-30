using Esri.ArcGISRuntime.Xamarin.Forms;
using RoutingSample.ViewModels;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace RoutingSample.Forms
{
    [DesignTimeVisible(false)]
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
            var status = await DependencyService.Get<ILocationAccessService>().RequestAsync();
            if (status == LocationAccessStatus.Granted)
            {
                await mapView.LocationDisplay.DataSource.StartAsync();
            }
            
            _mainViewModel.LocationDisplay = mapView.LocationDisplay;
            _mainViewModel.LocationDisplay.NavigationPointHeightFactor = 0.5;

            base.OnAppearing();
        }

        private void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            //_mainViewModel.Destination = mapView.ScreenToLocation(e.Position);
        }

        private async void ButtonSimulation_Clicked(object sender, EventArgs e)
        {
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
