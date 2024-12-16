using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Esri.ArcGISRuntime.Mapping;
using Location = Esri.ArcGISRuntime.Location.Location;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace BackgroundLocationTracking
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? logMessage;

        [ObservableProperty]
        private Map? webMapView;

        public MainPageViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Initialize the map with a basemap style
                WebMapView = new Map(BasemapStyle.ArcGISStreets);

                // Register to receive location updates
                WeakReferenceMessenger.Default.Register<Location>(this, (sender, location) => UpdateLocationNotification(location));

                // Start the location service
                LocationServiceManager.StartService();
            }
            catch (Exception ex)
            {
                // Handle initialization errors
                LogMessage = $"Error: An error occurred during initialization: {ex.Message}";
            }
        }

        private void UpdateLocationNotification(Location location)
        {
            if (location != null)
            {
                // Format the location update message
                var message = $"Lat: {location.Position.Y:F6}, Lon: {location.Position.X:F6}, Time: {DateTime.Now:HH:mm:ss}";

                // Log location update
                LogMessage += $"Location Update: {message}\n";
            }
        }
    }
}
