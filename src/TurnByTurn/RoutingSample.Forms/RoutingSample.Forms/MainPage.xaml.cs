using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Xamarin.Forms;
using RoutingSample.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
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
            // Make sure we have location permissions
            var status = await DependencyService.Get<ILocationAccessService>().RequestAsync();
            if (status == LocationAccessStatus.Granted)
            {
                await mapView.LocationDisplay.DataSource.StartAsync();
            }

            _mainViewModel.LocationDisplay = mapView.LocationDisplay;
            _mainViewModel.LocationDisplay.NavigationPointHeightFactor = 0.5;

            // Make sure user is signed in
            var credential = AuthenticationManager.Current.FindCredential(new Uri("https://www.arcgis.com/sharing/rest"));
            if (credential == null)
            {
                await Navigation.PushModalAsync(new LoginPage());
            }

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
