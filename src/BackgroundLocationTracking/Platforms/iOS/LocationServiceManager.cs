namespace BackgroundLocationTracking
{
    public static class LocationServiceManager
    {
        private static LocationService? _locationService;

        /// <summary>
        /// Starts the location service for iOS.
        /// </summary>
        public static async void StartService()
        {
            // Initialize and start the location service
            _locationService = new LocationService();
            await _locationService.Start();
        }

        /// <summary>
        /// Stops the location service.
        /// </summary>
        public static async void StopService()
        {
            if (_locationService != null)
            {
                // Stop the location service
                await _locationService.Stop();
                _locationService = null;
            }
        }
    }
}
